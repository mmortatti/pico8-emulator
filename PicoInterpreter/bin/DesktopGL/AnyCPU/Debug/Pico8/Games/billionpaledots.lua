pico-8 cartridge -- http://www.pico-8.com
version 16
__lua__
-- a billion pale dots
--   by jakub wasilewski

-- feel free to play with the
-- variables below to get
-- various results - dig
-- deeper into the
-- implementation to make
-- bigger changes.
-- you can also play with the
-- textures/lighting tables 
-- in the sprite memory.

-- center for 3d projection
center_x,center_y=64,56

------------------------------
-- technobabble
------------------------------

technobabble={
 "reversing polarity",
 "adjusting flux capacitor",
 "calculating flight plan",
 "charging ftl drive",
 "plotting course",
 "optimizing warp factors",
 "cooling element zero",
 "priming fuel pumps",
 "loading dilithium crystals",
 "heating ion clusters",
 "dialing the stargate",
 "consulting starcharts",
 "solving hyperspace equations",
 "scanning subspace radio",
 "triangulating starbases",
 "synchronizing ansibles",
 "decohering quantum state",
 "focusing guidance beam",
 "locating mass relay",
 "accounting for stellar drift"
}
for i=1,#technobabble do
 technobabble[i]={
  text=technobabble[i]
 }
end

------------------------------
-- utilities
------------------------------

-- rounds the number to the
-- nearest integer
function round(x)
 return flr(x+0.5)
end

-- linear interpolation from
-- a to b. t=0->a, t=1->b
function lerp(a,b,t)
 return a+(b-a)*t
end

-- random real number
-- between lo and hi
function rndf(lo,hi)
 return lo+rnd()*(hi-lo)
end

-- random number in range
-- with weighting towards the center
function rndw(lo,hi,center_weight)
 local sign,mag=
  rnd()>0.5 and 1 or -1,
  rnd()
 if center_weight==0.5 then
  fac=0.5+sign*sqrt(mag)*0.5
 else
  fac=0.5+sign*mag^center_weight*0.5
 end
 return lerp(lo,hi,fac)
end

-- picks randomly from a 
-- sequence
function rndpick(seq)
 return seq[flr(rnd(#seq)+1)]
end

-- ceiling to complement floor
function ceil(n)
 return -flr(-n)
end

-- prints some text with
-- a chosen alignment,
-- 0.5 - centre, 1 - right align
function printa(t,x,y,c,align)
 x = x - (align*4*#t)
 print(t,x,y,c) 
end

------------------------------
-- class & entity system
------------------------------

-- object is the master "class"
-- it has one method - extend(),
-- which can be used to make
-- new classes.
-- each class also inherits that
-- method, making hierarchies
-- possible.
--
-- new instances are made by
-- calling my_class:new({...}),
-- and the object passed to
-- new contains all properties
-- for the instance.
--
-- class objects can define a
-- create() method that gets
-- called whenever this happens.
--
object={}
 function object:extend(kob)
  -- remember the hierarchy
  kob=kob or {}
  kob.extends=self
  -- add the constructor method
  kob.new=function(self,ob)
   -- set up inheritance
   ob=setmetatable(ob or {},{__index=kob})
   -- call all create() methods
   -- in inheritance order
   local ko,create_fn=kob
   while ko do
    if ko.create and ko.create~=create_fn then
     create_fn=ko.create
     create_fn(ob)
    end
    ko=ko.extends
   end
   -- object ready
   return ob
 	end
 	-- set up inheritance between
 	-- the class objects themselves
 	return setmetatable(kob,{__index=self})
 end

-- returns a function that will
-- update all entities
-- passed in the parameter
function update_system(entities)
 return function()
  for e in all(entities) do
   e:update()
  end
 end
end

-- returns a function that will
-- render all entities
-- passed in the parameter
function render_system(entities)
 return function()
  -- depth-sort for correct
  -- drawing order
  local ordered={}
  for e in all(entities) do
   local o=flr(e.order)
   if o then
	   local tab=ordered[o] or {}
	   add(tab,e)
	   ordered[o]=tab
	  end
  end
  -- draw in z-order
  for z=-10,10 do
   if ordered[z] then
    for e in all(ordered[z]) do
     if e.render then
      e:render()
     end
    end
   end
  end
 end
end

------------------------------
-- 3d projection
------------------------------

-- sets the projection up,
-- tilted about x axis by
-- the angle provided
function init_projection(tilt)
 pryy,pryz,przy,przz=
  cos(tilt),sin(tilt),
  -sin(tilt),cos(tilt) 
end

-- projects a point (x,y,z) 
-- using an ortographic projection
-- also provides z, for depth
-- sorting purposes.
function project(x,y,z)
 local py,pz=
  pryy*y+przy*z,
  pryz*y+przz*z
 return center_x+x,center_y+py,pz
end

------------------------------
-- computing lighting luts
------------------------------

-- number of lighting levels
-- possible
lut_levels=8
-- the index of the "neutral"
-- lighting level - the one
-- that means "no change"
neutral=5

------------------------------

-- grabs the lighting look-up
-- tables from the sprite memory
-- see sprite memory at (40,64)
function init_blend_luts()
 -- palette table coords
 local blx,bly=40,64
 -- base addresses
 local addr
 local even_base,odd_base=
  0x4300,
  0x4300+lut_levels*0x100

 -- lookup tables for even lines
 for even_lut=0,lut_levels-1 do
  addr=even_base+0x100*even_lut
  for byte=0,255 do
   local c1,c2=
    band(byte,0xf),
    flr(shr(byte,4))
   local l1=sget(blx+even_lut*2,bly+c1)
   local l2=sget(blx+even_lut*2+1,bly+c2)
   poke(addr+byte,l1+l2*16)
  end
 end
 
 -- lookup tables for odd lines
 -- different, to achieve
 -- a cross-hatch pattern in
 -- some light levels
 for odd_lut=0,lut_levels-1 do
  addr=odd_base+0x100*odd_lut
  for byte=0,255 do
   local c1,c2=
    band(byte,0xf),
    flr(shr(byte,4))
   local l1=sget(blx+odd_lut*2+1,bly+c1)
   local l2=sget(blx+odd_lut*2,bly+c2)
   poke(addr+byte,l1+l2*16)
  end
 end
 
 blends={}
 for lut=0,lut_levels*2-1 do
  blends[lut]=fl_blend(lut)
 end
end

------------------------------
-- light blending function
------------------------------

-- returns a function that
-- applies the lighting
-- level "l" to a single
-- horizontal line segment.
function fl_blend(l)
 local lutaddr=0x4300+shl(l,8)
	return function(x1,x2,y)
	 -- this function operates
	 -- on screen memory directly
	 local laddr=lutaddr
	 local yaddr=0x6000+shl(y,6)
	 local saddr,eaddr=
	  yaddr+band(shr(x1+1,1),0xffff),
	  yaddr+band(shr(x2-1,1),0xffff)
	 -- odd pixel on left?
	 if band(x1,1.99995)>=1 then
	  local a=saddr-1
	  local v=peek(a)
	  poke(a,
	   band(v,0xf) +
	   band(peek(bor(laddr,v)),0xf0)
	  )
	 end
	 -- full bytes fast loop
	 for addr=saddr,eaddr do
	  poke(addr,
	   peek(
	    bor(laddr,peek(addr))
	   )
	  )
	 end
	 -- odd pixel on right?
	 if band(x2,1.99995)<1 then
	  local a=eaddr+1
	  local v=peek(a)
	  poke(a,
	   band(peek(bor(laddr,v)),0xf) +
	   band(v,0xf0)
	  )
	 end
	end
end

-------------------------------
-- palette effects
-------------------------------

-- creates palettes from
-- tables in sprite memory
-- (at (0,64))
function init_palettes(n)
 -- we keep palettes as
 -- blocks of memory ready
 -- to copy directly into
 -- pico-8 video state
 local addr=0x5800
 for plt=0,n-1 do
  for c=0,15 do
   poke(addr,sget(plt,64+c))
   addr = addr + 1
  end
 end
end

-- sets palette number "no"
function set_palette(no)
 -- modify the pico-8 video
 -- state directly, fast
 memcpy(0x5f00,
  0x5800+shl(flr(no),4),
  16)
end

function set_scr_palette(no)
 -- modify the pico-8 video
 -- state directly, fast
 memcpy(0x5f10,
  0x5800+shl(flr(no),4),
  16)
end

------------------------------
-- precomputing & drawing
-- the lighting
------------------------------

-- this object will be
-- responsible for applying
-- lighting to a planet
lighting=object:extend()
 function lighting:create()
  -- size to fit the planet
  local p=self.target
  local radius=p.size+1
  -- precalculate for later
  self:prepare(radius)
  self.slices=self:scan(radius)
 end

 -- draws the lighting on screen
 -- using normal drawing operations
 -- with each color corresponding
 -- to a light level 
 function lighting:prepare(radius)
  cls()
	 for c in all(self.def) do
	  circfill(
	   64+c[1]*radius,64+c[2]*radius,
	   c[3]*radius,
	   c[4]+1
	  )
	 end
 end
 
 -- remakes the light's circles
 -- into a set of horizontal
 -- light segments suitable
 -- for use with fl_blend
 function lighting:scan(radius)
	 radius = radius + 2
	 -- to support dimming the moon
	 -- when it's in earth's shadow,
	 -- we have several precomputed
	 -- tables for each "dimming"
	 -- level
	 local dlv=
	  self.target.dim_levels
	 local slices={}
	 for i=0,dlv do
	  slices[i]={}
	 end
	 -- scan the screen looking
	 -- for horizontal stretches
	 -- of similar lighting
	 for y=64-radius,64+radius do
	  local lutb=y%2==0
	   and 0 or lut_levels      
	  local prvc,sx=0  
	  for x=64-radius,64+radius+1 do
	   local c=pget(x,y)
	   if c~=prvc then
	    if prvc~=0 then
	     -- new light segment
	     -- store in all the
	     -- "dimming" level
	     -- tables
		    for i=0,dlv do
		     local lutn=max(prvc-1-i,0)
		     -- each slice gets a
		     -- pre-prepared function
		     -- to light it
		     add(slices[i],{
		      sx=sx-64,ex=x-64-1,y=y-64,
		      fl=blends[lutb+lutn]
		     })
		    end
	    end
	    sx,prvc=x,c
	   end
	  end
	 end
	 return slices
 end
 
 -- applies the lighting, with
 -- additional dimming, and at
 -- the right coordinates
 function lighting:apply(dimming,dx,dy,scale)
  if scale==1 then
   self:fast_apply(dimming,dx,dy)
   return
  end
  local ss=self.slices[dimming]
  local n=#ss
  local px,py=-1000
  for i=1,n do
   local s=ss[i]
   local sx,sy,ex=
    flr(s.sx*scale),
    flr(s.y*scale),
    flr(s.ex*scale)
   if py~=sy or sx>px then
    s.fl(sx+dx,ex+dx,sy+dy)
    px,py=sx,sy
   end
  end
 end
 
 function lighting:fast_apply(dimming,dx,dy)
  local ss=self.slices[dimming]
  local n=#ss
  for i=1,n do
   local s=ss[i]
   s.fl(s.sx+dx,s.ex+dx,s.y+dy)
  end
 end

------------------------------
-- helpers for 3d planet
-- precalc
------------------------------

-- does all the actual 3d
-- calculations for planets
-- fills up a "coords" table
-- that will be used to
-- get texturing info later
function prep_planet(p)
 local s=p.size
 local coords={}
 -- go through all latitudes
 for lat=-0.25,0.25,0.003 do
  -- the horizontal scale of 
  -- this "slice" of the
  -- earth sphere
  local scl=cos(lat)
  local sscl=s*scl
  -- texture coords
  local tox,toy,tw,th=
   p.tex_origin.x,
   p.tex_origin.y,
   p.tex_size.w,
   p.tex_size.h
  local tcy=toy+th/2
  -- go through all longitudes
  for long=-0.5,0.5,0.003/scl do
   -- 3d coordinates of this
   -- lat/long on the sphere
   local x,z,y=
    sscl*cos(long),
    sscl*sin(long),
    s*sin(lat)
   -- same coords, after 3d
   -- projection
   local fx,fy,fz=
    project(x,y,z)
   fx,fy=round(fx),round(fy)
   -- texture u/v coords
   -- for this point
   local tx,ty=
    flr(tox+long*tw%tw),
    flr(tcy-lat*2*th)
   -- visible?
   --  ("back-face culling")
   if fz>0 then
    if not coords[fy] then
     coords[fy]={}
    end
    -- store for scan_slices
    coords[fy][fx]={x=tx,y=ty}
   end
  end
 end
 return coords
end

-- takes the 3d data generated
-- by prep_planet and divides
-- the whole sphere into
-- horizontal line segments
-- that can be textured
-- with one sspr() call.
-- this is the whole trick
-- that makes this fast enough.
function scan_slices(p,coords)
 local s=p.size
 local slices={}
 for y,cc in pairs(coords) do
  local sty,sx,stx,ptx=
   nil,nil,nil
  for x=center_x-s-2,center_x+s+2 do
   local ty=cc[x] and cc[x].y
   if ty~=sty then
    if sty then
     local ssx,ssw=sx,x-sx
     if abs(ptx+128-stx)<abs(ptx-stx) then
      ptx = ptx + 128
     end
     if ptx<stx then
      ssx,ssw=ssx+ssw,-ssw
      ptx,stx=stx,ptx
     end
     add(slices,{
      sx=ssx-center_x,sw=ssw,sy=y-center_y,
      tx=stx,ty=sty,
      tw=ptx-stx+1
     })
    end
    sx,stx,sty=
     x,cc[x] and cc[x].x,ty
   end
   ptx=cc[x] and cc[x].x
  end
 end
 return slices
end

------------------------------
-- flight control
------------------------------

flightctrl=object:extend({
 order=5,
 flash=7,
 elapsed=0,
})
 function flightctrl:update()
  -- fly towards planet
  -- (if not yet there)
  local prevz=self.z
  self.z=lerp(self.z,1,0.1)
  if self.z<1.03 then self.z=1 end 
  -- propagate
  self.planet.z=self.z
  self.starfield.vz=max((prevz-self.z)*0.2,0.02)
  -- flash
  if self.flash>0 then
   self.flash = self.flash - 0.25
  end
  -- timekeeping
  self.elapsed = self.elapsed + (1/fps)
 end
 function flightctrl:render()
  set_scr_palette(flr(8+self.flash))
  if self.elapsed>6 then
   local pt=max(7-(self.elapsed-6)/0.05,0)
   set_palette(flr(pt))
   printa("press ❎ to warp",63,69+self.planet.size,13,0.5)
  end
 end

------------------------------
-- movable starfield
------------------------------

starfield=object:extend({
 order=0
})
 function starfield:create()
  self.stars={}
 end
 function starfield:update()
  if not self.vz then return end 
  for i=1,60 do
   local s=self.stars[i]
   if (not s) or s.oob then
    self.stars[i]=self:new_star()
   else
    s.z = s.z - (self.vz)
   end
  end
 end
 function starfield:new_star()
  local d,a=rndf(20,80),rnd()
  return {
   x=sin(a)*d,
   y=cos(a)*d,
   z=rndf(3,5)
  }
 end
 function starfield:render()
  if not self.stars[1] then return end 
  for i=1,#self.stars do
   local s=self.stars[i]
   local sz=max(s.z,0.01)
   local x,y=
    80+s.x/sz,48+s.y/sz
   if s.px then
    local mx,my=
     shr(x+s.px,1),
     shr(y+s.py,1)
    line(x,y,mx,my,5)
    line(mx,my,s.px,s.py,1)
   end
   s.px,s.py=x,y
   s.oob=x<0 or y<0 or x>128 or y>128
  end
 end

------------------------------
-- planet objects
------------------------------

-- the main object - a planet
-- is a sphere that can texture
-- itself and uses light/cloud
-- object to render aftereffects
planet=object:extend({
 order=1,
 x=0,y=0,dim=0,
 dim_levels=0,
 light_def={
	 {0,0,1.07,1},
	 {-0.02,-0.02,0.95,2},
	 {-0.05,-0.05,0.9,3},
	 {-0.08,-0.08,0.84,4},	 
	 {-0.15,-0.15,0.7,5},
	 {-0.2,-0.2,0.6,6},
	 {-0.3,-0.3,0.4,7},
	}
})
 function planet:create()
  -- initialize lighting
  self.light=lighting:new({
   target=self,
   def=self.light_def
  })
  cls()
  -- create the texturing data
  local crds=prep_planet(self,tlt)
  local slcs=scan_slices(self,crds)
  self.s=slcs
  -- texture mask (depends
  -- on texture width)
  self.tmsk=self.tex_size.w-0x0.0001 
  -- initialize clouds
  -- clouds rotate at different
  -- speed to separate the
  -- layers visually
  if self.cloud_count then
   self.clouds=clouds:new({
    target=self,
    count=self.cloud_count,
    speed=-128*
     self.rot/self.tex_size.w
   })
  end
 end
 
 function planet:update()
  -- rotate self
  self.off = self.off + (self.rot)
  self.off = self.off % (self.tex_size.w)
  -- ...and the clouds
  if self.clouds then
   self.clouds:move()
  end
 end
 
 function planet:render()
  if not self.z then return end 
  local scale=1/self.z
  if scale<1/self.size then return end 
  -- find our 2d center
  -- based on 3d coordinates
  local x,y=project(self.x,self.y,self.z)
  x,y=round(x),round(y)
  camera(-x,-y)  
  -- render all the textured
  -- slices
  local slices=self.s
  for si=1,#slices do
   self:render_slice(slices[si],scale)
  end
  -- add the clouds
  if self.clouds then
   self.clouds:overlay()
  end
  camera()
  -- light everything
  self.light:apply(self.dim,x,y,scale)
 end
 
 function planet:render_slice(s,scale)
  -- find the current texture
  -- coordinates for this slice
  -- they change to give
  -- illusion of rotating
  -- the planet is static,
  -- but the texture moves
  -- over its surface instead.
  local tsx=band(s.tx+self.off,self.tmsk)
  local tex=band(tsx+s.tw,self.tmsk)
  -- texture wrapping
  if tex<tsx then
   -- this slice is an unhappy
   -- case that needs 2 sspr()
   -- calls.
   local tw1,tw2=
    self.tex_size.w-tsx,
    tex+1
   local scaled_w,scaled_x,scaled_y=
    -flr(-s.sw*scale),flr(s.sx*scale),s.sy*scale
   local sw1=round(scaled_w*tw1/(tw1+tw2))
   local sw2=scaled_w-sw1
   local sx1,sx2=scaled_x,scaled_x+sw1
   sspr(tsx,s.ty,tw1,1,
    sx1,scaled_y,sw1,1)
   sspr(0,s.ty,tw2,1,
    sx2,scaled_y,sw2,1)
  else
   -- happy case - the whole
   -- slice fits into a single
   -- texture repetition
   local scaled_w,scaled_x,scaled_y=
    -flr(-s.sw*scale),
    s.sx*scale,
    s.sy*scale
   sspr(tsx,s.ty,tex-tsx+1,1,
    scaled_x,scaled_y,scaled_w,1)
  end
 end

------------------------------
-- main loop
------------------------------

fps=60
debug=false
function _init() 
 init_palettes(16)
 init_blend_luts()
 init_projection(0.05)

 new_planet()
end

function _draw()
 if not debug then
	 cls()
	 set_palette(0)
	 if render_entities then render_entities() end 
	end
end

function new_planet()
 cls()
 flip()

 _update60,render_entities=
  nil,nil

 -- reset technobabble 
 for tb in all(technobabble) do
  tb.used=false
 end
 
 -- pick a planet type
 local planet_types={
  {name="earthlike",rng=0.75,generate=generate_earthlike},
  {name="gaseous",rng=1,generate=generate_gaseous}
 }
 local r,planet_type=rnd()
 for pt in all(planet_types) do
  if r<pt.rng then
   planet_type=pt
   break
  end
 end
 
 -- generate the planet
 local pd,renderer=
  planet_type.generate() 
 
 -- sneak peek
 printh("=====================")
 printh(planet_type.name)
 for k,v in pairs(pd) do
  if type(v)~="table" then
   printh(k..": "..v.."    ")
  end
 end

 -- render the texture
 render_texture(
  renderer,
  debug and pset or sset,
  0,0
 )
 --cstore()
 
 -- pass onto the function
 -- that will actually make
 -- entities
 -- this inconvenient handover
 -- is for memory-saving
 -- purposes
 _update60=function()
  actually_make_planet(pd)
 end
end

function actually_make_planet(pd)
 -- adapt to chosen fps
 local multiplier=
  60/fps  
 local entities={}
 
 -- create planet object
 if not debug then
  -- mask preparations
  set_scr_palette(16)
  -- generate entities
	 local gaia=planet:new({
	  size=pd.size,
	  off=0,rot=pd.rotation*multiplier,
	  tex_origin={x=0,y=0},
	  tex_size={w=128,h=64}
	 })  
	 local stars=starfield:new()
  local flight=flightctrl:new({
   planet=gaia,starfield=stars,
   z=1000
  })
  -- add to list
		add(entities,flight)
	 add(entities,gaia)
		add(entities,stars)
	end	
 
 -- create the update/render
 -- loops with these entities
 update_fn=
  make_update_fn(entities)
 render_entities=
  render_system(entities)
 
 -- use the right _update
 -- variant for the fps
 if fps==30 then
  _update=update_fn
 else
  _update60=update_fn
 end
end

function make_update_fn(entities)
 local update_entities=update_system(entities)
 return function()
  update_entities()
  if btnp(4) or btnp(5) then new_planet() end 
 end
end

-------------------------------
-- simplex noise
-------------------------------

-- various constants and helpers
-- for the simplex noise gen

function v3d(x,y,z)
 return {x=x,y=y,z=z}
end

function v3ddot(grad,x,y,z)
 return grad.x*x+grad.y*y+grad.z*z
end

grad3={
  [0]=v3d(1,1,0),v3d(-1,1,0),v3d(1,-1,0),v3d(-1,-1,0),
  v3d(1,0,1),v3d(-1,0,1),v3d(1,0,-1),v3d(-1,0,-1),
  v3d(0,1,1),v3d(0,-1,1),v3d(0,1,-1),v3d(0,-1,-1)
}

ps={[0]=151,160,137,91,90,15,
131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180}

perm,perm12={},{}
for i=0,511 do
 perm[i]=ps[band(i,0xff)]
 perm12[i]=perm[i]%12
end
skew,unskew=1/3,1/6
unskew2,unskew3=2*unskew,3*unskew

-- the main noise generator function
function simplex3d(x,y,z)
 local n0,n1,n2,n3
 local s=(x+y+z)*skew
 local i,j,k=
  flr(x+s),flr(y+s),flr(z+s)
 local t=(i+j+k)*unskew
 local xo,yo,zo=i-t,j-t,k-t
 local xd,yd,zd=x-xo,y-yo,z-zo

 local i1,j1,k1,i2,j2,k2
 if xd>=yd then
  if yd>=zd then
   i1,j1,k1,i2,j2,k2=
    1,0,0,1,1,0
  elseif xd>=zd then
   i1,j1,k1,i2,j2,k2=
    1,0,0,1,0,1
  else
   i1,j1,k1,i2,j2,k2=
    0,0,1,1,0,1
  end
 else
  if yd<zd then
   i1,j1,k1,i2,j2,k2=
    0,0,1,0,1,1
  elseif xd<zd then
   i1,j1,k1,i2,j2,k2=
    0,1,0,0,1,1
  else
   i1,j1,k1,i2,j2,k2=
    0,1,0,1,1,0
  end
 end

 local x1,y1,z1=
  xd-i1+unskew,
  yd-j1+unskew,
  zd-k1+unskew
 local x2,y2,z2=
  xd-i2+unskew2,
  yd-j2+unskew2,
  zd-k2+unskew2
 local x3,y3,z3=
  xd-1+unskew3,
  yd-1+unskew3,
  zd-1+unskew3
 local ii,jj,kk=
  band(i,0xff),band(j,0xff),band(k,0xff)
 local gi0,gi1,gi2,gi3=
  perm12[ii+perm[jj+perm[kk]]],
  perm12[ii+i1+perm[jj+j1+perm[kk+k1]]],
  perm12[ii+i2+perm[jj+j2+perm[kk+k2]]],
  perm12[ii+1+perm[jj+1+perm[kk+1]]]
 local t0=0.6-xd*xd-yd*yd-zd*zd
 if t0<0 then
  n0=0
 else
  t0 = t0 * t0
  n0=t0*t0*v3ddot(grad3[gi0],xd,yd,zd)
 end
 local t1=0.6-x1*x1-y1*y1-z1*z1
 if t1<0 then
  n1=0
 else
  t1 = t1 * t1
  n1=t1*t1*v3ddot(grad3[gi1],x1,y1,z1)
 end
 local t2=0.6-x2*x2-y2*y2-z2*z2
 if t2<0 then
  n2=0
 else
  t2 = t2 * t2
  n2=t2*t2*v3ddot(grad3[gi2],x2,y2,z2)
 end
 local t3=0.6-x3*x3-y3*y3-z3*z3
 if t3<0 then
  n3=0
 else
  t3 = t3 * t3
  n3=t3*t3*v3ddot(grad3[gi3],x3,y3,z3)
 end
 return 32*(n0+n1+n2+n3)
end

function noisegen(seed,scale_x,octaves,scale_y)
 if not scale_y then scale_y=scale_x end 
 local base_m=
  2^(octaves-1)/(2^octaves-1)
 return function(x,y,z)
  local n,m,sx,sy=
   0,base_m,scale_x,scale_y
  for o=1,octaves do
   n = n + (m*simplex3d(x*sx,y*sy,z*sx+seed))
   sx,sy,m=
    shl(sx,1),shl(sy,1),shr(m,1)
  end
  return n
 end  
end

-------------------------------
-- temperature
-------------------------------

-- simple temperature map
-- hotter at equator
-- colder at poles and high up
function assign_temperature(
 height,pd
)
 local average,variation=pd.temp_average,pd.temp_var
 local equator,pole=average+variation,average-variation
 local water_level=pd.water_level
 local factor,ctr_y={},31.5*(1+pd.temp_ctr)
 for y=0,63 do
  factor[y]=abs(y-ctr_y)/31.5
 end
 return function(x,y)
  local asl=height[x][y]-water_level
  if asl<0 then return average end 
  local base=lerp(
   equator,pole,factor[y]
  )
  return base-asl*0.5
 end
end

-------------------------------
-- terrain
-------------------------------

-- all terrain types, with
-- texturing, bumpmapping data
t_water,t_deep,t_land,t_forest,
t_desert,t_rock,t_snow,t_shrubland=
 {tx=112,ty=96,bump=-0.5,flat=true},
 {tx=32,ty=96,bump=-0.5,flat=true},
 {tx=96,ty=96,bump=0},
 {tx=80,ty=96,bump=0.1},
 {tx=64,ty=96,bump=0},
 {tx=48,ty=96,bump=0.2},
 {tx=112,ty=112,bump=0.2},
 {tx=96,ty=112,bump=0}

-- assigns terrain to each
-- pixel based on various
-- maps we prepared
function assign_terrain(
 height,vegetation,temperature,
 pd
)
 local water_range=1+pd.water_level
 local rock_limit=
  lerp(1,pd.water_level,pd.rockiness)
 return function(x,y)
  local h=height[x][y]
  if h>pd.water_level then
   local t=temperature[x][y]
   if t<-0.3 then
    return t_snow
   elseif t<-0.1 then
    return t_shrubland
   end
   if h>rock_limit then
    -- mountains more bumpy
    height[x][y]=rock_limit+(h-rock_limit)*2
    -- it's rock anyway
    return t_rock
   end
   local v=vegetation[x][y]*pd.vege_var+
    pd.vege_level
   if v<-0.3 then
    return t_desert
   elseif v>0.3 then
    return t_forest
   else
    return t_land
   end
  else
   local depth=(pd.water_level-h)/(1+pd.water_level)
   local deep_p=depth*0.4
   return rnd()<deep_p and
    t_deep or t_water
  end
 end
end

-------------------------------
-- renderers
-------------------------------

-- renders terrain textures,
-- applies lighting on the fly
function terr_render(
 terrain,light
)
 return function(x,y)
  local t,l=
   terrain[x][y],light[x][y]
  local ox,oy=band(x,15),band(y,15)
  local clr=sget(t.tx+ox,t.ty+oy)
  return sget(8+l,104+clr)
 end
end

-- renders a <-1,1> texture
-- using a simple ramp
function ramp_render(tex,ramp_y)
 return function(x,y)
  local v=tex[x][y]
  return sget(8+v*8,ramp_y)
 end
end

-- renders a gaseous planet
-- uses a table of colors
-- to which values in the
-- base texture are mapped
function gas_render(base,colors)
 local ccount=#colors
 local lo,hi=0.5,ccount+0.5
 return function(x,y)
  local v=base[x][y]+rndf(-0.1,0.1)
  local ci=mid(1,ccount,
   lerp(lo,hi,(v+1)*0.5))
  return colors[round(ci)]
 end
end

-------------------------------
-- emboss
-------------------------------

-- pixels used for the
-- emboss filter
emboss_conv={
 {dx=-1,dy=-1,m=-0.33},
 {dx=-1,dy=0,m=-0.33},
 {dx=0,dy=-1,m=-0.33},
 {dx=1,dy=1,m=0.33},
 {dx=1,dy=0,m=0.33},
 {dx=0,dy=1,m=0.33}
}

-- samples a texture at x,y
-- wrapping/clipping properly
function sample(tex,x,y)
 x,y=
  band(x,127),mid(y,0,63)
 return tex[x][y]
end

-- applies a 3d lighting effect
-- prebaked into a texture
function emboss(height,terrain,starkness)
 return function(x,y)
  local t=terrain[x][y]
  if t.flat then
   return 0
  end
  local here=height[x][y]+t.bump
  local v=0
  for e in all(emboss_conv) do
   local diff=
    sample(height,x+e.dx,y+e.dy)+
    sample(terrain,x+e.dx,y+e.dy).bump-
    here
   v = v + (diff*e.m*starkness)
  end
  return mid(v,-8,8)
 end
end

-------------------------------
-- generation
-------------------------------

color_starters={
 1,2,3,4,8,9,11,14
}
color_neighbours={
 [0]={1},
 {0,2,5,3},
 {1,4,8},
 {5,11},
 {2,9,8},
 {1,3},
 {5,7},
 {6,10,15},
 {9,14,4,2},
 {8,10,4,15},
 {9,7,11,15},
 {3,7,10,15},
 {},
 {},
 {15,8,9},
 {10,7,14}
}
function generate_gaseous_pd()
 local colors={rndpick(color_starters)}
 for i=2,5 do
  colors[i]=rndpick(color_neighbours[colors[i-1]])
 end
 
 local pd={
  colors=colors,
  smear=rndw(0.05,0.45,2),
    
  seed=rnd(255),
  size=round(rndw(30,34,1)),
  rotation=0.5
 }
 return pd
end

function generate_gaseous()
 local pd=generate_gaseous_pd()
 -- generate maps
 local base=texture(
  noise(pd.seed,pd.smear,3,3)
 )
 -- return renderer
 return pd,gas_render(base,pd.colors)
end

function generate_earthlike_pd()
 local pd={
  water_level=rndw(-0.9,0.55,3),
  vege_level=rndw(-1.3,1.3,1),
  vege_var=1-abs(rndw(-1,1,3)),
  rockiness=rndw(0.1,0.9,3),
  temp_average=rndw(-1,1,2),
  temp_var=rndw(0,1,2),
  temp_ctr=rndw(-0.5,0.5,3),
  
  seed=rnd(255),
  size=round(rndw(27,33,1)),
  rotation=0.5,
 }
 return pd
end

function generate_earthlike()
 -- generate planetary data
 local pd=generate_earthlike_pd()
 -- generate maps
 local height=texture(
  noise(pd.seed,0.96,3)
 )
 local vegetation=texture(
  noise(pd.seed+1,0.64,1)
 )
 local temperature=texture(
  assign_temperature(height,pd)
 )
 local terrain=texture(
  assign_terrain(
   height,vegetation,temperature,
   pd
  )
 )
 local light=texture(
  emboss(height,terrain,20)
 )
 
 -- render the final texture
 --return pd,ramp_render(temperature,96)
 return pd,terr_render(
  terrain,light
 ) 
end

-- generates 2d noise texture
-- with proper wrapping
function noise(...)
 local ng=noisegen(...)
 return function(tx,ty)
	 local lng,lat=
	  tx/128,(ty-31.5)/126
	 local y,scl=-sin(lat),cos(lat)
	 local x,z=scl*sin(lng),scl*cos(lng)
	 return ng(x,y,z)
 end
end

-- generates a texture using
-- an f(x,y) function
function texture(fn)
 local tex={}
 cls()
 rectfill(16,64,111,64,1)
 
 local tb
 repeat
  tb=rndpick(technobabble)
 until not tb.used
 printa(tb.text,64,56,13,0.5)
 tb.used=true
 
 for x=0,127 do
  tex[x]={}
  for y=0,63 do
   tex[x][y]=fn(x,y)
  end
  pset(16+x*0.75,64,8)
 end
 return tex
end

-- renders a texture generically
-- using a mapping function and
-- an output function
function render_texture(color_fn,out_fn,xs,ys)
 for x=0,127 do
  for y=0,63 do
   out_fn(xs+x,ys+y,color_fn(x,y))
  end
 end 
end


__gfx__
00000000100000000000000000000000000000000000000001111111000000000000000000000000000000000000000111110110100100111000011011110000
00000000000000000000000011111111100000000111111111111111100000000000000000000000000000000000000001111000100001011110101101100000
00000000000000000000111111111111111111111111111117777711111000100000001111110000001110000000000011111111111010011111111110001000
00000000100000000111111111171177777776661777777777777777660111111000167761110100000110100000001117601111101111111001110100011100
1100011111110000111111111117777776666000666677777767767660111111000017600111000001100111000001111001bb00111111111111111101011101
1111111111111111111177711111777677600111000067776777676601d1100000001001111111001111b011111111bbbbbbb33b011111111111111110111111
1111111111111111111177777771777766666611111107777767766011d110000001111111000011111b0111bb0bbbbb33333333bbbbb3011bbbbb1111111111
b111113bbbbbbb11bbbdd7ddddd1166666f00f66111d107667766601111111000011111bbbbb0b013bbbbbbb33b33b33333b3333333333bbbbb3333bbbbbbb33
331013bb3b33b1bbbbbbbbbb3bbbb11fff0111ff601d11177666000bb330100001111bb30b33bbb3b333b3333333b33333b333b333bb3b333333bb33bbbbb333
00011bb33331111bbbbbbbb33bb333330011111601dd11166600111133000001111bbb00bbbbbbb333333333333bb33b3333333333333333b333333333333330
11111bb31000033bbbbbbb333b33b3001111176011d11100601101010000111111bbb301b3b33333333b3333bb33b3333333b333b33333333333113333113001
111101000111100003bbbbbbb33bb33011111bb1bb11d11101101001011111b01100111bb3333333bb3b3333333b3333333333333333333bb300000031001111
11111100011111111033bb33bb3bb33333011b3b3331d1111011111000111b33011bbbbb33b33b33b3333393333333333b333333333bb3333001111330111111
1100000011111111110bbb33ff933333b3bbb33331331110000101010101111301bbb333333333333333b9993933933333333333333333b33b30111001111101
11010000110110111110bb3b9f99333333333331000b30110110100000111010bbb33bbb3333333339f9f99fff999339f9393393993333333301111110011000
10000000100010011111bbb99ff993b33333333330100011000110000000111b3333b33333000033919fffffff99339ffffffffff93333331101111110011000
10000000000010101111ff9f4f9499333b33333001d111100000000001011bb33000003b310111033119ffff99333fff9fff9f944553b33100b0111110000100
00000000000111111111ff994ff993333333330111d1110000000000001117f901111103101331133119fffff33f9ffff9999945533bb3001101111111100000
000000100000111111111f94ff94493b3333301dd111110000000000111117f011111103019999f99119fff9939fff9f999344453330b3011b01011101000000
0000001000000000111111ffff944933333301dd111101000000100110111001777111101111199ff9ffff99433ff9f9994333333330130b3300110010000011
00000101000000011100110ff9f9933113301dd1100101000000000001111177ff9911177111fff9fff9ff944933999944333bb3333010100010100000111111
000000000000000110000110f994330000b011d10001000000000001111117fffff9977ff941ffff90fff99fff933344453b3333b31010110111110001001111
00000000000000000100100109443011d1030111001110000000000011177fffffffffffff940ffff01119999f9933bb33bb3b33310001011110000000001111
0001100000001001000100101049301dd110111110011110000000001179ffffff9ffffff9f40ff9f90901119933b33333333333101110001011100000011101
000000000000011000000001110b3011b011b0111101011000000000117ffff9f99fff9f99f940fff99990111bb333100bbb3300011110000000100000000000
0000000000001100000001001110b33b311d01301d11100000000001117fff9ffffff9ffffff400ff999011111b3310111bb3301111111001000100000000000
0000000000011100000000100101000b330111011111111000000011117999ffff9ffffffff994009900111111bb3011111b3301111b00010001000010110000
0000000000001001010000000011111003301111111d11110000000011b33999999999993333334000111111111b301100100330111301110001110001111000
0000000000000110110000000100101110330bbb1b11d11111100000110bbb93939339333bbbb33409111010011b101000011301111130110000000001111000
000000000000001000100000100010111100bbbbb3b11111111100000010333bb3b3333bb3333334091100000110010000011011111130000000010011110000
000000000000000010100011010110101111bbb33333301d111100000011000000b3bb3bbbb3939401101000010010000011b0111b0110000100100011100000
000000000010100000010111100010110101bb33b3333301d11110000001111111013bbbb33399401111000000000000011b301bb30111111111000000010000
00000000001000000000101100000001100bbb3bb333b3301011100000011111111b333bbb3944011111111000000000011130133013011b1110000000001100
000000001000000000000111000101001110b33b33b3333330111000001111010111b3bbb339440111110101000000101001030001330111bbb3010000001011
0000000101100000000001010000111001111b333333333499301000001011111111133333994401110100100000000000101013010011110333100100111011
0000000011000000000000100000010000001bb33b33b349443011100000010101111f3933944401111110000000000000001111111111111000011011011100
00000000010000001100000000000000000111b333334494430110000000000100111f99999440b01111110100100001000001111111111f1111111001110000
10000010000000000000000000000010001111fb33349f9f43011101000000011011f9ff994440301b011100010000001000001101111fff11f1111011111111
0000000000000000000000000000010110111109933349f943010000000000000111f9ff94440101b01110011100000001000001101fff999ff9011000111111
000000000000000000000000000000001111111f93b34944301110100000000000011f999444011b30000100000000000000010000f999f9f999901000111100
000000000000000001000110011110111110111f93339443011100010000000000011f9f9444011b300000100011000000001101199f99999ff9940100011010
000000000000110000000000000101100111111f493333301011111000000000001111ff94401110010000001001011000001101099994449f99443001100010
0000000000010000010100101111011000011119499333301111010000001010000011f9944011111110000000000111000010111b9444244994433111100111
001000001000000001011000001010011101111f9494400111110111000011100101111444011111111001000000111100000000103100004444310010110001
00100000100000000010000000011010000101194994011100111100000101100101011000111110111010000000001110000000110011110023101101010111
100000001000000000000100000010100011119f94401111001000000000100000001111111111101001001000000000000001001111111110b30111111001b0
00000000000000000000000000010000101111f94001111101011100000010100001101111010010100000000000000100100001010101111100111111111b30
00000000000000000000010001000000110111f94011111011110000010010110001100011011000000011000000000000000000011101001101111110113001
10000000100000000000001000000001100011f901dd100000001001101100011100100111101000110000110000000001000000010001101001111111100111
00000000001000000000000001000000000011f90d00000000000000000000100000000000000000010010000000000100000000010000111000000001110110
00000000010110000000100000000000000011901000000000100000011000000010000000000000000000010000000000000010000000010000010010100101
00000000100100000000000000100000000001010000000000000000000000000000000100000010001000000010010001110111100001001000010011100111
00000000000000000000000001100001000000100000000110000000001000000001100010101100000010101111000111100010101000011001100000000111
00000000000000000000000001010000011111000000000000000000000000001000010100011100000010001110110101111111110110110001101100000000
00000000000000000000000000001000001111101000000000000000000000011000100010111101110000011111111111011111011111100111111100000000
00000000000000000000000000111110010001100110000000001000000100111000011111111111110101111011111111111110001101001111110011100000
00000000000000100100000010111111011111110111000000011110001001001111011111111111111111111111111111100001111111101111110110111111
00000000000000111011111111110110110111117701100000111000100101100101111111111111111111111111177777777777777777777777771111111111
10011100010110111111111111111111111117777600110001011101111111111777777777777777777776607777777777777777777777777777777776011111
11111111111111111777777011117777770177777660111000111100111777777777777777777777777777777777777777777777777777777777677760111111
77777777777777777777777777777777777777777767777777777777777777777766777777777777777777777777777777667777777777777666677777777777
77777666777777777766677777777767777777766677777666777777677777766667777777777666777777667777776666667777777766666666666677777777
66666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666
66666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666
0000000000001d700000000000000000000000000000000000001011000000000000000000000000000000000000000000000000000000000000000000000000
1100000011115d7000000000000000000000000000000010115155dd000000000000000000000000000000000000000000000000000000000000000000000000
2110000022449a700000000000000000000000000000101122424444000000000000000000000000000000000000000000000000000000000000000000000000
331100003399aa700000000000000000000000000010113133b3bbbb000000000000000000000000000000000000000000000000000000000000000000000000
442210004499aa7000000000000000000000000000101122444499aa000000000000000000000000000000000000000000000000000000000000000000000000
5511000055dd667000000000000000000000000000001151553533b3000000000000000000000000000000000000000000000000000000000000000000000000
66dd510066777770000000000000000000000000000155dd66667677000000000000000000000000000000000000000000000000000000000000000000000000
776dd510777777700000000000000000000000000155dd6677777777000000000000000000000000000000000000000000000000000000000000000000000000
88822100888ee7700000000000000000000000000111222888e8eee7000000000000000000000000000000000000000000000000000000000000000000000000
99942100999aaa700000000000000000000000000001224499aa7a77000000000000000000000000000000000000000000000000000000000000000000000000
aa994210aa77777000000000000000000000000010112499aa7a7777000000000000000000000000000000000000000000000000000000000000000000000000
bbb33100bbaaa770000000000000000000000000001133b3bbabaaa7000000000000000000000000000000000000000000000000000000000000000000000000
ccdd5110ccc77770000000000000000000000000001155ddcccccccc000000000000000000000000000000000000000000000000000000000000000000000000
dd511000dd66667000000000000000000000000000101155dd667677000000000000000000000000000000000000000000000000000000000000000000000000
ee882210eeee777000000000000000000000000000112288eeeee777000000000000000000000000000000000000000000000000000000000000000000000000
fff94210fffff77000000000000000000000000001224499ff7f7777000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00011224499aa7777000000000000000ddddddddddddddddddddddddddddddddffffffff9fffffffbb355005bb55bbb3bbbbbbbbbbbbbbbbcccccccccccccccc
0015dccc333b9a8e7000000000000000ddddddddddddddddddddddddddddddddffffffffffffffffb335003bb335bb33bbbbbbbbbbbbbbbbcccccccccccccccc
00000000000000000000000000000000ddddddddddddddddddddddddddddddddff999ffffff9ffff535bb33533305530bbbbbbbbbbbbbbbbcccccccccccccccc
00000000000000000000000000000000ddddddddddddddddddddddddddddddddfffffffffff99fff55bb53bbb500b500bbbbbbbbbbbbbbbbcccccccccccccccc
00000000000000000000000000000000ddddddddddddddddddddddddddddddddffffffffffffffff3bb355bb35bbb35bbbbbbbbbbbbbbbbbcccccccccccccccc
00000000000000000000000000000000ddddddddddddddddddddddddddddddddffffff99ffffffff5b3335b33053b303bbbbbbbbbbbbbbbbcccccccccccccccc
00000000000000000000000000000000ddddddddddddddddddddddddddddddddffffffffffff99ff305300350bb55005bbbbbbbbbbbbbbbbcccccccccccccccc
00000000000000000000000000000000ddddddddddddddddddddddddddddddddffffffffffffffff355005550b355bbbbbbbbbbbbbbbbbbbcccccccccccccccc
00000000000000000000000000000000dddddddddddddddddddddddddddddddd999fffffffffffff53bbb5bb5530bbb3bbbbbbbbbbbbbbbbcccccccccccccccc
0001011111151555d000000000000000ddddddddddddddddddddddddddddddddfff9fffffffff99f3bb3b0b355005b33bbbbbbbbbbbbbbbbcccccccccccccccc
00000000200000000000000000000000ddddddddddddddddddddddddddddddddffffff99ffff99ff305330330bb30533bbbbbbbbbbbbbbbbcccccccccccccccc
011155533333bbbaa000000000000000ddddddddddddddddddddddddddddddddfffffff99fffffff35bb5bb00b300000bbbbbbbbbbbbbbbbcccccccccccccccc
00000000400000000000000000000000ddddddddddddddddddddddddddddddddf9ffffffffffffff5bb3bb30550bb50bbbbbbbbbbbbbbbbbcccccccccccccccc
0011151555333b3bb000000000000000dddddddddddddddddddddddddddddddd99fffffffffffff9b5530b5bb5bb00bbbbbbbbbbbbbbbbbbcccccccccccccccc
0155ddd6666777777000000000000000ddddddddddddddddddddddddddddddddffff99ffffffffffbb000b3b33b00553bbbbbbbbbbbbbbbbcccccccccccccccc
01dd6667777777777000000000000000ddddddddddddddddddddddddddddddddffffffffff99ffff3353b33b335bbbb0bbbbbbbbbbbbbbbbcccccccccccccccc
001122288888eeef70000000000000000000000000000000000000000000000000000000000000000000000000000000b66b66bb666bbb667777777767777777
0001244999fffff770000000000000000000000000000000000000000000000000000000000000000000000000000000bb666666bb6b66b67777777777777777
00000000a000000000000000000000000000000000000000000000000000000000000000000000000000000000000000b6b6b6bbbb6b6bbb7766677777767777
0155333bbbbbabaa700000000000000000000000000000000000000000000000000000000000000000000000000000006bb666b66b6bb66b7777777777766777
00000000c0000000000000000000000000000000000000000000000000000000000000000000000000000000000000006bb6bb6bb6b6b6b67777777777777777
01111ddddd666777700000000000000000000000000000000000000000000000000000000000000000000000000000006b6bb6666b6bbbbb7777776677777777
0122888eeeeefff770000000000000000000000000000000000000000000000000000000000000000000000000000000bbbbbb6bb666bb667777777777776677
0124499fffffff7770000000000000000000000000000000000000000000000000000000000000000000000000000000b6b6bb6bbbbb66667777777777777777
000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000666b66bbbbb666666667777777777777
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000006666b6b66b6b66b67776777777777667
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000006b6bbb66b6b66b6b7777776677776677
000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000bb66b66b66666bb67777777667777777
000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000b66bbb6b66666b667677777777777777
000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000b6b6bb6bb66b66b66677777777777776
000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000b666bbb6b66bbb6b7777667777777777
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000006bb6bb6bbbbb6bbb7777777777667777
__label__
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000010000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000000000000000000000000000000000000001335d3142440000000000000000000000000000000000000000000000000000000000
000000000000005000000000000000000000000000000000000000155d133b3d3bdb114ddd000000000000000000000000000000000000000000000000000000
0000000000000005100000000000000000000000000000000011105bdb667b66b6ddb12264d11000000000000000000000000000000000000000000000000000
00000000000000000110000000000000000000000000000001201b63bbb6bbb6dddd3313396d1100000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000194011bb3b6bb66b6bbbbb333b3b35502101000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000102444015b36776666667aababbbbbbb3b13211100000000000000001000000000000000000150000000
00000000000000000000000000000000000000000001244240015b66ababaa7666a77666bbbb3131331200000000000000000000000000000000000000000000
00000000000000000000000000000000000000000142949af0245167676767666ab666d1bb13b31b3b3322010000000000000000000000000000000000000000
00000000000000000000000000000000000000000ff9ff7f7743501015567676516666500050353bb3b314411000000000000000000000000000000000000000
0000000000000000000000000000000000000000fffff77777ab51015557550105551500000101353b3b31441200000000000000000000000000000000000000
000000000000000000000000000000000000005fff7a7777aaaabbbb5555101050335100005105033353b1134220000000000000000000000000000000000000
000000000000000000000000000000000000050ff7f7777aaa7a7a7ab3b303550133550005030350003b33333412000000000000000000000000000000000000
000000000000000000000000000000000000509f7f7777a7a7a7a7a7a7b3131bb313551051350050000bb3b33322200000000000000000000000000000000000
000000000000000000000000000000000005c9f777777a77777a7a7bbbdb130bb30355553b1b0035bb0bb53b3244220000000000000000000000000000000000
00000000000000000000000000000000005cff77777777a77777a3a3bbdbb11b30101553b5bbb0350b0bbbb3b925422000000000000000000000000000000000
0000000000000000000000000000000000cdf7f777777a77777aba3bbbbb31d30ab50b353bba333bbb0053b33bd5142000000000000000000000000000000000
0000000000000000000000000000000006cf7f77777da7a77777abbdb1a3a7b31ba01aaa5a1b33b5bb05bb01b3bd115100000000000000000000000000000000
00000000000000000000000000000000577ff7773b7dba7d7ddbbb3d317a3a7b7a01aaaa5ca10a3b335bbb03333b135550000000000100000000000000000000
0000000000000000000000000000000477ff7777b3bbb7a7db13bb1111aba7bba11333baa01013bbb35bbbb3b1b3b33d50000000001000000000000000000000
00000000000000000000000000000009fff7777a7bbb7bbddbb13b3bbb3bbbbb3aaaaa0bbbbbab3bbb355bbb533b3b3d10000000000000000000000000000000
0000000000000000000000000000004fff777777a7aba3bbbbbbb3bbbbb3bbb3b7aaa0bbbbba330053b553bb53b3b334d1000000000000000000000000000000
0000000000000000000000000000009ff7f777777b3b3bbbbb3bbb3dd111dabb3bbbbbbaabb331001b333b05533b3b3340000000000000000000000000000000
0000000000000000000000000000049fff77767777b31dbbbdbdb3bdd111d7b3b3bbbbbaabb3b310b33101055533b3333d000000000000000000000000000000
0000000000000000000000000000049ff677777d31bb1ddd113ddb31113a7a3b3bbb333abbb3010b5b150100353b3b1033100000000000000000000150000000
000000000000000000000000000009f97f7777d7d311bdddd11111bbb3b3a7a3b3bbbaaaaab33bb5b3513cc03533b33033210000001000000000000000000000
00000000000000000000000000004949f66767ddbb11bbd1111111bbbbbb3b3b311aaaaaabb3bbbaba3b5cc0dbbb3b1003d50000000000000000000000000000
0000000000000000000000000000994fa67677b3dbbbbdd11111cbb3b7a7bbb3b1a7aaaaaaaaaaaaabb5abbdbcbbb330d5551000000000000000000000000000
00000000000000000000000000012947f76767b13bbbb11cc7111cbb3bba7bbbbb7aaaaaaaaaaaaaba3ababbccbb3b13055d5000000000000000000000000000
0000000000000000000000000001199f75167db1b3bbdcc1ccc7ccb3b7b7abb3b7a7aaaaaaaaaaaaa3b3abbbcc3cb350055d5000000000000000000000000000
0000000000000000000000000000999a35010ddbbb3bddcccc7777cbbb7abbbb7a7aaaaaaaaaaaab3b3abab33b3ccb35015d5000000000000000000000000000
00000000000000000000000000009ffaa51010ddb3dd11cccccc7777b3a3bba7a7aaaaaaaaaaaaabb3b3abb3350cb3b0d11d5500000000000000000000000000
00000000000000000000000000049ffabb5101dddbda111ccccc77773a7bbb7a7a7aaaaaaaaaaaaa3b3bba3350cccd30dd15d000000000000000000000000000
00000000000000000000000000149ffbbb5013b1d3d7a7bccccccc7111bbd7a7a7aaaaaaaaaaaaaab3b3bb3335cccdb3dd55d500000000000000000000000000
000000000000000000000000001bfffb3b35013b1bba7a3ccccc111bdd7a7a7a7aaaaaaaaaaaaaaa3b3b3abb35cccd3ddd55d100000000000000000000000000
0000000000000000000000000039f9f3bb335b1311bbb11cccccc111b3d7a7a7a7aaaaaaaaaaaaaba3bbabbb35bbcd63dd55d110000000000000000000000000
000000000000000000000000003bf39b3b33bb013bbbbb1ccccccbdbbada7a7a7aaaaaaaaaaaaaab3abababbbbbba66bdd5dd100000000000000000000000000
0000000000000000000000000031f433b333bbaa13b3bdc7ccccccb3ddb7a7a7aaaaaaaaaaaaaaabb3bbabbbbbbba663bd3dd110000000000000000000000000
0000000000000000000000000035033b3bbbabaa01dbbbddc77cccccddba7a7aaaaaaaaaaaaaaaab3abafa9bbbbbbb3b3b3d3110000000000000000000000000
000000000000000000000000001d0133bbabaaaaa01dbbb71cccccccc7a7a7bbbaaaaaaaaaaaaa7aaf7fa9bbbffbb3b3b3332110000000000000000000000000
0000000000000000000000000010c1333bbaaaaabb013bba711cccccc77bbbbbbaaaaaaaaaaaa7aaf7faaf999ffffb3b99333100000000000000000000000000
000000000000000000000000001dd933bbaaaaabbbb010111cccccccc77bbbbbbaaaaaaaaaaaa77f7f7f7f9fffffb3b399333110000000000000000000000000
000000000000000000000000001ddc9b3abaaaabbbbb33cc01ccccc77701bbaaaaaaaaaaaa7777f7f7f7ffffffff993b9b333100000000000000000000000000
0000000000000000000000000015dcf9bbabaaaaaabbb3ccccccc77b77701baaaaaaaaaaaaaa7a7f7f7fffffffff99b9b3333000000000000000000000000000
0000000000000000000000000015dccffababaaaaaaaa7ccccccccc7bb01aaaaaaaabbaaa777a7f7f7f7ffffffff999b33333100000000000000000000000000
0000000000000000000000000015dd7fffababaaaaaaaa77acccc7cc7bb0aaaaaaaabaaa777aaa7f7f7fffff99f99491b3331010000000000000000000000000
0000000000000000000000000005d6c7fbbababaaaa77aa7aaccccc773aaaaaaaabbbbb7a7aaf7f7f7ffd4499992494343330000000000000000000000000000
0000000000000000000000000001dd77ff9babaaaa77777777101cc733aaaaaaaaabbbbb7aaf7f7f7ffd442999449994b3330000000000000000000000000000
00000000000000000000000000035697fff9babaaaa777a77a7701077bbaaaaaaaaaaaaa77f7f7f7a99222999924994433310100000000015000000000000000
0000000000000000000000000001d66fffffabababa777aa77aaa77aaaaaaaaaaaaaaaaf7f7f7f7aa99222492449999933300100000000000000000000000000
00000000000000000000000000004d99fffff7bababaa777777aaa77aaaaaaaaaaaaaaf7f7f7f7aa944444992224999433110000000000000000000000000000
00000000000000000000000000004499ffffff7bababaa77777aaaa01033aaaaaaaaaf7f7faaaa44444499994244499443101000000000000000000000000000
000000000000000000000000000054969ffffff7baba77777777aaaa0103aaaaaaaabaf7f7faaa44444499992224499433010000000000000000000000000000
00000000000000000000000000000449667fffff7baa7767777776aaaaaaaaaaa3b3b3aa7f7aa444444222922241444431100000000000000000000000000000
0000000000000000000000000000054966777ffff7b7a76666666aaaaaaaaababb3b3ba7faf72000044222944114994310000000000000000000000000000000
0000000000000000000000000000014d666777777f7776666666666bababababb3b3b4aa7f4000004490cc4490d9944110110000000000000000000000000000
00000000000000000000000000000005d66677777777667666666676babababa3b3b34aa2000cccc990ccdd900ddd41101000000000000000000000000000000
00000000000000000000000000000011d66d6777777776666666666766aba3b3b3b3a24000cccccccfccdd9d62ddd21110000000000000000000000000000000
000000000000000000000000000000011dd5dd67777776d66666666666663b33533b200ccccccccfccfdddd62d9d241100000000000000000000000000000000
00000000000000000000000000000002051d555d6777766dd15166666666b3b535b3200ccccccccccfdddd6999d4411110000000000000000000000000000000
000000000000000000000000000000001551155566776d10ddd1156666651015555000cccccccccff69666999444431110000000000000000000000000000000
00000000000000000000000000000000005510555666d001000115ddd1d11011555000ccccccccc7699999994444311100000000000000000000000000000000
000000000000000000000000000000000001113555516d00005333bddd1100055000ccccccccccdd699999944443111100000000000000000000000000000000
00000000000000000000000000000000001011135555d10000005333bbd11000550ccddccccccddd669999244431111000000000000000000000000000000000
000000000000000000000000000000000001001151555100500333bbbd000110501ccccccccdddd6699992444311110000000000000000000000000000000000
000000000000000000000000000000000000100005151010005bbbbb3555000151cccccccdddddd6999412443111100000000000000000000000000000000000
000000000000000000000000000000000000000000015100001bbbbbbb355533399fccdddddd6669994122431111000000000000000000000000000000000000
0000000000000000000000000000000000000010200020005131b3bbbbb333333bb4996666666669942244311110000000000000000000000000000000000000
0000000000000000000000000000000000000000142202001b133b3b3b3b13131b44999999999999444443111100000000000000000000000000000000000000
0000000000000000000000000000000000000000012442200031b3b3b1313133b3b3b99999499944444311110000000000000000000000000000000000000000
000000000000000000000000000000000000000001022422231b3b33331313133b3b999924424444333111110000000000000000000000000000000000000000
0000000000000000000000000000000000000000000102242423b33133b3b3b3b3b33331b3344433311110000000000000000000000000000000000000000000
000000000000000000000000000000000000000000001122444333333b3b3b3b3b3b311133324331511000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000005222433333333333333333333324d3d551510000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000002222153333335111135535533d151555500000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000050101153533135353331111555500000000000000000000000000000000000000000000000000
00000010000000000000000000000000000000000000000000000010101111151151555555100000000000000000000000000000000000000000000000000000
00001100000000000000000000000000000000000000000000000000001111155505555000000000000000000000000000000000000000000000000000000000
00010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00500000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
05000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000100000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000005100000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000000000000ddd0ddd0ddd00dd00dd000000ddddd000000ddd00dd00000d0d0ddd0ddd0ddd00000000000000000000000000000000
000000000000000000000000000000000d0d0d0d0d000d000d0000000dd0d0dd000000d00d0d00000d0d0d0d0d0d0d0d00000000000000000000000000000000
000000000000000000000000000000000ddd0dd00dd00ddd0ddd00000ddd0ddd000000d00d0d00000d0d0ddd0dd00ddd00000000000000000000000000000000
000000000000000000000000000000000d000d0d0d00000d000d00000dd0d0dd000000d00d0d00000ddd0d0d0d0d0d0000000000000000000000000000000000
000000000000000000000000000000000d000d0d0ddd0dd00dd0000000ddddd0000000d00dd000000ddd0d0d0d0d0d0000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000001000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000

