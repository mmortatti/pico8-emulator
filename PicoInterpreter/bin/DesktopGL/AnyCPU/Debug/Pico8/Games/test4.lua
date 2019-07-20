pico-8 cartridge // http://www.pico-8.com
version 16
__lua__

debug=false

function deep_copy(obj)
 if (type(obj)~="table") then return obj end
 local cpy={}
 setmetatable(cpy,getmetatable(obj))
 for k,v in pairs(obj) do
  cpy[k]=deep_copy(v)
 end
 return cpy
end
function shallow_copy(obj)
  if (type(obj)~="table") then return obj end
 local cpy={} 
 for k,v in pairs(obj) do
  cpy[k]=v
 end
 return cpy
end


-- creates a new object by calling obj = object:extend()
object={}
function object:extend(kob)
  kob=kob or {}
  kob.extends=self
  return setmetatable(kob,{
   __index=self,
   __call=function(self,ob)
	   ob=setmetatable(ob or {},{__index=kob})
	   local ko,init_fn=kob
	   while ko do
	    if ko.init and ko.init~=init_fn then
	     init_fn=ko.init
	     init_fn(ob)
	    end
	    ko=ko.extends
	   end
	   return ob
  	end
  })
end

vector={}
vector.__index=vector
 -- operators: +, -, *, /
 function vector:__add(b)
  return v(self.x+b.x,self.y+b.y)
 end
 function vector:__sub(b)
  return v(self.x-b.x,self.y-b.y)
 end
 function vector:__mul(m)
  return v(self.x*m,self.y*m)
 end
 function vector:__div(d)
  return v(self.x/d,self.y/d)
 end
 function vector:__unm()
  return v(-self.x,-self.y)
 end
function vector:__neq(v)
  return not (self.x==v.x and self.y==v.y)
end
function vector:__eq(v)
  return self.x==v.x and self.y==v.y
end
 -- dot product
 function vector:dot(v2)
  return self.x*v2.x+self.y*v2.y
 end
 -- normalization
 function vector:norm()
  return self/sqrt(#self)
 end
 -- length
 function vector:len()
  return sqrt(#self)
 end
 -- the # operator returns
 -- length squared since
 -- that's easier to calculate
 function vector:__len()
  return self.x^2+self.y^2
 end
 -- printable string
 function vector:str()
  return self.x..","..self.y
 end

-- creates a new vector with
-- the x,y coords specified
function v(x,y)
 return setmetatable({
  x=x,y=y
 },vector)
end


-------------------------------
-- entity: base
-------------------------------

entity=object:extend(
  {
    t=0,
    spawns={}
  }
)

 -- common initialization
 -- for all entity types
function entity:init()  
  if self.sprite then
   self.sprite=deep_copy(self.sprite)
   if not self.render then
    self.render=spr_render
   end
  end
end
 -- called to transition to
 -- a new state - has no effect
 -- if the entity was in that
 -- state already
function entity:become(state)
  if state~=self.state then
   self.state,self.t=state,0
  end
end
-- checks if entity has 'tag'
-- on its list of tags
function entity:is_a(tag)
  if (not self.tags) then return false end
  for i=1,#self.tags do
   if (self.tags[i]==tag) then return true end
  end
  return false
end
 -- called when declaring an
 -- entity class to make it
 -- spawn whenever a tile
 -- with a given number is
 -- encountered on the level map
function entity:spawns_from(...)
  for tile in all({...}) do
   entity.spawns[tile]=self
  end
end

function entity:draw_dit(ct,ft,flip)
  draw_dithered(
      ct/ft,
      flip,
      box(self.pos.x+self.hitbox.xl,
      self.pos.y+self.hitbox.yt,
      self.pos.x+self.hitbox.xr,
      self.pos.y+self.hitbox.yb)
      )
end


-------------------------------
-- collision boxes
-------------------------------

-- collision boxes are just
-- axis-aligned rectangles
cbox=object:extend()
 -- moves the box by the
 -- vector v and returns
 -- the result
 function cbox:translate(v)
  return cbox({
   xl=self.xl+v.x,
   yt=self.yt+v.y,
   xr=self.xr+v.x,
   yb=self.yb+v.y
  })
 end

 -- checks if two boxes
 -- overlap
 function cbox:overlaps(b)
  return
   self.xr>b.xl and
   b.xr>self.xl and
   self.yb>b.yt and
   b.yb>self.yt
 end

 -- calculates a vector that
 -- neatly separates this box
 -- from another. optionally
 -- takes a table of allowed
 -- directions
function cbox:sepv(b,allowed)
  local candidates={
    v(b.xl-self.xr,0),
    v(b.xr-self.xl,0),
    v(0,b.yt-self.yb),
    v(0,b.yb-self.yt)
  }
  if type(allowed)~="table" then
   allowed={true,true,true,true}
  end
  local ml,mv=32767
  for d,v in pairs(candidates) do
   if allowed[d] and #v<ml then
    ml,mv=#v,v
   end
  end

  return mv
end
 
 -- printable representation
 function cbox:str()
  return self.xl..","..self.yt..":"..self.xr..","..self.yb
 end

-- makes a new box
function box(xl,yt,xr,yb) 
 return cbox({
  xl=min(xl,xr),xr=max(xl,xr),
  yt=min(yt,yb),yb=max(yt,yb)
 })
end

-------------------------------
-- entity: dynamic
-------------------------------

dynamic=entity:extend({
    maxvel=1,
    acc=0.5,
    fric=0.5,
    vel=v(0,0),
    dir=v(0,0)
  })

function dynamic:set_vel()  
  if (self.vel.x<0 and self.dir.x>0) or
     (self.vel.x>0 and self.dir.x<0) or
     (self.dir.x==0) then     
    self.vel.x=approach(self.vel.x,0,self.fric)
  else
    self.vel.x=approach(self.vel.x,self.dir.x*self.maxvel,self.acc)
  end

  if (self.vel.y<0 and self.dir.y>0) or
     (self.vel.y>0 and self.dir.y<0) or
     (self.dir.y==0) then
    self.vel.y=approach(self.vel.y,0,self.fric)
  else
    self.vel.y=approach(self.vel.y,self.dir.y*self.maxvel,self.acc)
  end
end

-------------------------------
-- entity: enemy
-------------------------------

enemy=dynamic:extend({
  collides_with={"player"},
  tags={"enemy"},
  hitbox=box(0,0,8,8),
  c_tile=true,  
  inv_t=30,
  ht=0,
  hit=false,
  sprite=26,
  draw_order=4,
  death_time=15,
  health=1,
  give=1,
  take=1,
  ssize=1,
  svel=0.1
})

enemy:spawns_from(19)

function enemy:init()
  if self.sprite==19 then self.ssize=2 end
end

function enemy:update()
  self.ht=self.ht+1

  if (self.hit) then self.dir=v(0,0) end

  if self.ht > self.inv_t then
    self.hit=false
    self.ht=0
  end
end

function enemy:dead()
  if self.t > self.death_time then
    mset(self.map_pos.x,self.map_pos.y,0)
    self.done=true
  end
end

function enemy:damage(s)
  if not self.hit then
    self.health=self.health-1
    s=s or 1
    add_time(self.give*s)
    p_add(ptext({
      pos=v(self.pos.x-10,self.pos.y),
      txt="+"..self.give*s,
      lifetime=45
      }))
    self.hit=true
    shake=5
    self.ht=0    
  end

  if (self.health <=0) then self:become("dead") end
end

function enemy:collide(e)
  if (self.state=="dead")then  return end
  if e:is_a("player") and e.damage then
    if e:damage() then
      local d=v(e.pos.x-self.pos.x,e.pos.y-self.pos.y)
      if #d>0.01 then d=d:norm() end
      e.vel=d*3
      add_time(-self.take)
    end
  end
end

function enemy:render()
  if (self.hit and self.t%3==0) then return end
  local s=self.sprite
  s=s+(self.t*self.svel)%self.ssize
  spr(s,self.pos.x,self.pos.y)

  if self.state=="dead" then
    self:draw_dit(self.t, self.death_time, true)
  end
end

-------------------------------
-- entity: blob
-------------------------------

blob=enemy:extend({
  state="moving",
  vel=v(0,0),
  hitbox=box(1,3,7,8),  
  maxvel=0.5,
  spd=1,
  health=1
})

blob:spawns_from(7)

function blob:init()
  self.fric=0.05
  self.ssize=3
  self.svel=0.15
end

function blob:moving()
  if not scene_player then return end

  self.dir=v(scene_player.pos.x-self.pos.x,
             scene_player.pos.y-self.pos.y):norm()

  self.maxvel = self.spd*(cos(self.t/20)+1)/2 + 0.1

  self:set_vel()
end

-------------------------------
-- entity: spike
-------------------------------

spike=entity:extend({
  state="first",
  collides_with={"player"},
  hitbox=box(0,0,7,7),
  time=30,
  draw_order=1
})

spike:spawns_from(115, 116)

function spike:init()
  if(self.sprite==116) then self:become("second") end
  self.mode=self.sprite==115 and 0 or 1
  self.sprite=115
end

function spike:first()
  if (self.t > self.time) then self:become("second") end
end

function spike:second()
  local s,p=c_get_entity(self),c_get_entity(scene_player)
  if (self.t > self.time and self.mode==0) or 
     (self.active and not p.b:overlaps(s.b) and self.mode==1) then
    self:become("third")
  end
end

function spike:third()
  if self.t==1 then
    for i=1,8 do
      local s=smoke({
        pos=v(self.pos.x+4+rnd(8)-4,self.pos.y+4+rnd(8)-4),
        vel=v(rnd(0.5)-0.25,-(rnd(1)+0.5)),
        r=rnd(0.7)+0.5
        })      
      e_add(s)
    end
    shake=shake+1
  end
  if (self.t > self.time and self.mode==0) then self:become("first") end
end

function spike:render()
  if (self.state=="second")then spr(self.sprite,self.pos.x,self.pos.y) end
  if (self.state=="third" )then spr(self.sprite+1,self.pos.x,self.pos.y) end
end

function spike:collide(e)
  if self.state=="third" then
    if e:damage() then
      local d=v(e.pos.x-self.pos.x,e.pos.y-self.pos.y)
      if #d>0.001 then d=d:norm() end
      e.vel=d*3
    end
  elseif self.mode==1 then
    self.active = true
  end
end

-------------------------------
-- entity: laser dude
-------------------------------

laserdude=enemy:extend(
  {
    state="wondering",
    vel=v(0,0),
    hitbox=box(-4,-4,4,4),
    health=4,
    give=4,
    take=2,
    fric=0.07,
    r=5
  }
)

laserdude:spawns_from(10)

function laserdude:shooting()
  self.vel=v(0,0)

  local llength=5
  if self.t==10 then
    shake=shake+5
    for i=0,llength-1 do
      local l=laser({dir=v(0,-1),pos=v(self.pos.x-self.r+1,self.pos.y-self.r-i*8)})
      l.lifetime=10+i
      e_add(l)
      l=laser({dir=v(0,1),pos=v(self.pos.x-self.r+1,self.pos.y+self.r+i*8)})
      l.lifetime=10+i
      e_add(l)
      l=laser({dir=v(1,0),pos=v(self.pos.x+self.r+i*8,self.pos.y-self.r+1)})
      l.lifetime=10+i
      e_add(l)
      l=laser({dir=v(-1,0),pos=v(self.pos.x-self.r-i*8,self.pos.y-self.r+1)})
      l.lifetime=10+i
      e_add(l)
    end
  end
  if self.t > 30 then
    self:become("wondering")
  end
end

function laserdude:wondering()
  local wonder_time=60
  if self.t > wonder_time and not self.hit then
    self:become("shooting")
  end

  if self.t == 1 then
    self.dir=v(rnd(2)-1,rnd(2)-1)*0.5
  end

  self:set_vel()
end

function laserdude:render()
  if self.hit and self.t%3==0 then return end
  circ(self.pos.x,self.pos.y,self.r,9)
  print("\130",self.pos.x-3,self.pos.y-2,9)

  if self.state=="dead" then
    self:draw_dit(self.t,self.death_time,true)
  end
end

laser=entity:extend(
  {
    hitbox=box(0,0,8,8),
    give=4,
    take=2,
    dir=v(1,0),
    collides_with={"player","oldman"},
    c_tile=false
  }
)

function laser:init()
  if (not self.lifetime) then self.lifetime=10 end
  self.hitbox=box(0,0,8,8)

  if self.dir.x<0 then
    self.hitbox.xl=-8
    self.hitbox.xr=0
  end

  if self.dir.y<0 then
    self.hitbox.yt=-8
    self.hitbox.yb=0
  end

  if self.dir.x~=0 then
    self.hitbox.yt=3
    self.hitbox.yb=5
  end

  if self.dir.y~=0 then
    self.hitbox.xl=3
    self.hitbox.xr=5
  end
end

function laser:update()
  if (self.t>self.lifetime) then self.done=true end
end

laser.collide=enemy.collide

function laser:render()
  rectfill(self.hitbox.xl+self.pos.x,self.hitbox.yt+self.pos.y,
           self.hitbox.xr+self.pos.x,self.hitbox.yb+self.pos.y,9)
  if self.t >= 3*self.lifetime/4 then
    self:draw_dit((self.lifetime-self.t),(self.lifetime/4),false)    
  end
end

-------------------------------
-- entity: bullet
-------------------------------

bullet=dynamic:extend({
    collides_with={"player"},
    tags={"bullet"},
    hitbox=box(-1,-1,1,1),
    maxvel=2,
    c_tile=true,
    lifetime=30,
    r=3
})

function bullet:init()
  self.wo=rnd(1)
end

function bullet:update()
  self.set_vel(self)

  if self.t%5==0 then
    local s=smoke({
        pos=v(self.pos.x+rnd(2)-1,self.pos.y+rnd(2)-1),      
        c=rnd(1)<0.5 and 7 or 9})
    s.vel=v(rnd(1)-0.5,rnd(1)-0.5)
    p_add(s)
  end

  -- self.pos.x+= self.dir.y*sin(self.t/5 + self.wo)
  -- self.pos.y+= self.dir.x*sin(self.t/5 + self.wo)

  if (self.t > self.lifetime) then self.done=true end
end

function bullet:render()
  circfill(self.pos.x,self.pos.y,self.r,9)
end

function bullet:collide(e)
  for i=1,2 do
    p_add(smoke({
      pos=v(self.pos.x+4+rnd(2)-1,self.pos.y+2+rnd(2)-1),
      c=rnd(1)<0.5 and 7 or 9
    }))
  end
  if e:is_a("player") and e.damage then
    if e:damage() then
      e.vel=self.dir*3
    end
  end
  self.done=true
end

function bullet:tcollide()
  for i=1,2 do
    p_add(smoke(
    {
      pos=v(self.pos.x+4+rnd(2)-1,self.pos.y+2+rnd(2)-1),
      c=rnd(1)<0.5 and 7 or 9
    }
  ))
  end
  self.done=true
end


-------------------------------
-- entity: old man
-------------------------------

oldman=enemy:extend({
  state="idle",
  tags={"oldman"},
  vel=v(0,0),
  hitbox=box(0,1,8,8),
  maxvel=0.5,
  ssize=3,
  svel=0.1,
  draw_order=2
})

oldman:spawns_from(1)

function oldman:init()
  self.thinkchar="\138"
  self.sprite=0
  self.dial=nil
end

function oldman:idle()
  self:set_vel()

  if (not scene_player) then return end
  local ec,oc=c_get_entity({pos=self.pos,hitbox=box(-8,-8,16,16)}),c_get_entity(scene_player)
  if ec.b:overlaps(oc.b) then
    if not self.dial then
      self.dial=dialog({pos=self.pos+v(0,-6),text="if i lose one more second i'll die!"})
      e_add(self.dial)
    end
  else
    if (self.dial) then self.dial:destroy() end
    self.dial=nil
  end
end

function oldman:collide(e)
 return c_push_out
end

-------------------------------
-- entity: player
-------------------------------

player=dynamic:extend({
  state="walking", vel=v(0,0),
  collides_with={"slowdown"},
  tags={"player"}, dir=v(1,0),
  hitbox=box(2,3,6,8),
  c_tile=true,
  sprite=0,
  draw_order=3,
  fric=0.5,
  inv_t=30,
  ht=0,
  hit=false,
  dmg=1,
  has_swrd=false,
  basevel=1
})

player:spawns_from(32)

function player:init()
  self.last_dir=v(1,0)
  add(lightpoints, self)
end

function player:destroy()
  del(lightpoints, self)
end

function player:update()
  self.ht=self.ht+1
  if self.ht > self.inv_t then
    self.hit=false
    self.ht=0
  end
end

function player:walking()
  self.dir=v(0,0)

  if self.hit and self.ht<self.inv_t/2 then self:set_vel() return end

  if btn(0) then self.dir.x = -1 end
  if btn(1) then self.dir.x =  1 end
  if btn(2) then self.dir.y = -1 end
  if btn(3) then self.dir.y =  1 end

  self:set_vel()

  -- correct diagonal movement
  if self.vel.x ~= 0 and self.vel.y ~= 0 then
    self.vel=self.vel/1.4
  end

  if (self.dir~=v(0,0)) then self.last_dir=v(self.dir.x,self.dir.y) end

  if btnp(4) then 
    self:become("attacking")
  end
  self.maxvel = self.basevel
end

function player:attacking()
  if not self.attk then
    local dir=self.last_dir.x~=0 and v(self.last_dir.x,0) or v(0, self.last_dir.y)
    self.attk=sword_attack(
      {
        pos=self.pos+dir*8,
        facing=dir,
        upg=self.has_swrd
      }
    )
    self.attk.dmg=self.dmg
    e_add(self.attk)    
  end

  self.vel=v(0,0)
  if self.attk.done then 
    self.attk=nil 
    self:become("walking")
  end
end

function player:render()
  if (self.hit and self.t%3==0) then return end

  local st=self.vel==v(0,0) and "idle" or "walking"
  local flip=false
  local spd=st=="idle" and 0 or 0.15
  self.sprite=st=="idle" and 32 or 33
  if self.last_dir.x<0 then
    flip=true
  end
  self.sprite=self.sprite+flr(self.t*spd)%4

  spr(self.sprite, self.pos.x, self.pos.y, 1, 1, flip)
end

function player:damage()
  if not self.hit then
    p_add(ptext({
      pos=v(self.pos.x-10,self.pos.y),
      txt="-1"
    }))
    self.ht=0
    self.hit=true

    return true
  end  
end

function player:collide(e)
  if e:is_a("slowdown") then
    self.maxvel=self.basevel/2
  end
end

-------------------------------
-- entity: sword_attack
-------------------------------

sword_attack=entity:extend(
  {
    lifetime=10,
    hitbox=box(0,0,8,8),
    tags={"attack"},
    collides_with={"enemy"},
    facing=v(1,0),
    dmg=1,
    sprite=3,
    draw_order=5
  }
)

function sword_attack:init()
  if self.upg then
    for i=1,4 do
      e_add(smoke({
            pos=v(self.pos.x+rnd(8),self.pos.y+rnd(8)),
            vel=v(rnd(0.5)-0.25,-(rnd(1)+0.5)),
            c=rnd(1)<0.7 and 12 or 7,
            v=0.15
        }))
    end
  end
end

function sword_attack:update()
  self.flipx=self.facing.x==-1
  self.flipy=self.facing.y==1

  if self.facing.x ~= 0 then self.sprite=3 else self.sprite=4 end
  if self.t > self.lifetime then self.done=true end

  self.hitbox=nil
end

function sword_attack:render()  
  spr(self.sprite, self.pos.x, self.pos.y, 1, 1, self.flipx, self.flipy)

  local nf=v(abs(self.facing.y),abs(self.facing.x))
  local off=v(abs(self.facing.x),abs(self.facing.y))*4+v(4,4)
  local pos=self.pos+nf*2
  if self.t >= 3*self.lifetime/4 then
    self:draw_dit((self.lifetime-self.t),(self.lifetime/4))    
  end
end

function sword_attack:collide(e)
  if e:is_a("enemy") and not e.hit then
    e:damage(self.dmg)
    local allowed_dirs={
      v(-1,0)==self.facing,
      v(1,0)==self.facing,
      v(0,-1)==self.facing,
      v(0,1)==self.facing
    }
    return c_push_vel,{allowed_dirs,1}
  end
end

-------------------------------
-- entity: sword upgrade
-------------------------------

sword_upgrade=entity:extend({
  hitbox=box(0,0,8,8),
  collides_with={"player"},
  draw_order=5
})

sword_upgrade:spawns_from(4)

function sword_upgrade:update()
  if self.t%2==0 then
    e_add(smoke({
          pos=v(self.pos.x+rnd(8),self.pos.y+rnd(8)),
          c=rnd(1)<0.7 and 12 or 7,
          v=0.15
      }))
  end
end

function sword_upgrade:collide(e)
  e.dmg=e.dmg*2
  e.has_swrd=true
  self.done=true
end

-------------------------------
-- entity: speed upgrade
-------------------------------

speed_upgrade=entity:extend({
  hitbox=box(0,0,8,8),
  collides_with={"player"},
  draw_order=5,
})

speed_upgrade:spawns_from(51)

function speed_upgrade:init()
  self.orig = v(self.pos.x, self.pos.y)
end

function speed_upgrade:update()
  if self.t%2==0 then
    e_add(smoke({
          pos=v(self.pos.x+rnd(8),self.pos.y+rnd(8)),
          c=rnd(1)<0.7 and 12 or 7,
          v=0.15
      }))
  end

  self.pos = self.orig+v(0, 3*sin(self.t/40+0.25))
end

function speed_upgrade:render()
  spr(self.sprite + (self.t*0.05)%2, self.pos.x, self.pos.y)
end

function speed_upgrade:collide(e)
  e.basevel=e.basevel*1.5
  self.done=true
end

-------------------------------
-- entity: bat
-------------------------------

bat=enemy:extend({
  hitbox=box(1,0,8,5),
  collides_with={"player"},
  draw_order=5,
  state="idle",
  attack_dist=60,
  vel=v(0,0),
  maxvel=0.5,  
  fric=2,
  acc=2,
  health=1,
  c_tile=false
})

bat:spawns_from(55)

function bat:init()
  self.orig = v(self.pos.x, self.pos.y)
end

function bat:idle()
  if not scene_player then return end
  local dist=sqrt(#v(scene_player.pos.x-self.pos.x,
             scene_player.pos.y-self.pos.y))
  if dist < self.attack_dist then 
    self:become("attacking")
    self.sprite=53
    self.ssize=2
    self.svel=0.05
  end
end

function bat:attacking()
  if not scene_player then return end

  self.dir=v(scene_player.pos.x-self.pos.x,
             scene_player.pos.y-self.pos.y):norm()  

  self:set_vel()

  self.pos = self.pos+v(0, 0.5*sin(self.t/40+0.5))
end

-------------------------------
-- entity: pot
-------------------------------
pot=entity:extend({
  hitbox=box(0,0,8,8),
  collides_with={"player","attack"},
  state="notbroke"
  })
pot:spawns_from(17)

function pot:collide(e)
  if (e:is_a("player")) then return c_push_out end

  if e:is_a("attack") and self.state~="broke"then
    shake=shake+2
    self.sprite=18
    self:become("broke")
    mset(self.map_pos.x,self.map_pos.y,0)
    self.hitbox=box(0,3,8,8)
    self.collides_with={}
    if rnd(1)<0.1 then
      e_add(slowmo_obj({pos=self.pos+v(0,-5)}))
    end
    -- e_add(spart({sprite=self.sprite,b=box(0,0,4,4),pos=self.pos}))
    -- e_add(spart({sprite=self.sprite,b=box(0,4,4,8),pos=self.pos+v(0,1)}))
  end
  
end

-------------------------------
-- entity: spart
-------------------------------
spart=entity:extend({

})

function spart:render()
  local y=flr(self.sprite/16)
  local x=self.sprite-y*16
  for j=self.b.xl,self.b.xr-1 do
    for i=self.b.yt,self.b.yb-1 do
      local p=sget(j+x*8,i+y*8)
      pset(self.pos.x+j,self.pos.y+i,p)
    end
  end
end

-------------------------------
-- entity: slowdown
-------------------------------
slowdown=entity:extend({
  hitbox=box(0,0,8,8),
  tags={"slowdown"},
  draw_order=2
  })
slowdown:spawns_from(56)

-------------------------------
-- entity: fireplace
-------------------------------

fireplace=entity:extend(
  {
    fr=2,ff=2,
    draw_order=2
  }
)

fireplace:spawns_from(98)

function fireplace:init()
  add(lightpoints, self)
  self.fr=4
end

function fireplace:destroy()
  del(lightpoints, self)
end

function fireplace:update()
  p_add(smoke(
    {
      pos=v(self.pos.x+4+rnd(2)-1,self.pos.y+2+rnd(2)-1),
      c=rnd(1)<0.5 and 7 or 9
    }
  ))
end


-------------------------------
-- entity: chimney
-------------------------------

chimney=entity:extend(
  {
  }
)

chimney:spawns_from(76)

function chimney:update()
  if self.t%3==1 then
    p_add(smoke(
      {
        pos=v(self.pos.x+4+rnd(2)-1,self.pos.y+rnd(2)-1),
        r=rnd(0.95)+1
      }
    ))
  end
end

-------------------------------
-- entity: birb
-------------------------------

bird=entity:extend(
  {
    sing=0,
    draw_order=6
  }
)

bird:spawns_from(114)

function bird:update()
  if self.t%15==self.sing then
    self.sing=flr(rnd(10)+5)
    p_add(ptext(
      {
        pos=v(self.pos.x-4,self.pos.y-4),
        vh=true,
        txt="\141"
      }
    ))
  end
end

-------------------------------
-- level loading
-------------------------------

level_index=v(4,1)

function load_level()
  old_ent=shallow_copy(entities)  
  e_add(level({
    base=v(level_index.x*16,level_index.y*16),
    pos=v(level_index.x*128,level_index.y*128),
    size=v(16,16)
  }))
end

level=entity:extend({
 draw_order=1
})
 function level:init()
  -- start with a lit area. any light_switches will make the room dark
  enable_light=false
  local b,s=
   self.base,self.size
  for x=0,s.x-1 do
   for y=0,s.y-1 do
    -- get tile number
    local blk=mget(b.x+x,b.y+y)    
    -- does this tile spawn
    -- an entity?
    local eclass=entity.spawns[blk]
    if eclass then
     -- yes, it spawns one
     -- let's do it!
     local e=eclass({
      pos=v(b.x+x,b.y+y)*8,
      vel=v(0,0),
      sprite=blk,
      map_pos=v(b.x+x,b.y+y)
     })     

     -- register the entity
    if e:is_a("player") and not scene_player then
      scene_player=e
      mset(b.x+x,b.y+y,0)
    end
    e_add(e)
     -- replace the tile
     -- with empty space
     -- in the map
     --mset(b.x+x,b.y+y,0)
     blk=0
    end
   end
  end
 end
 -- renders the level
 function level:render()
  map(self.base.x,self.base.y,
      self.pos.x,self.pos.y,
      self.size.x,self.size.y,0x1)
 end

-------------------------------
-- camera
-------------------------------

shake=0

cam=entity:extend(
  {
    tags={"camera"},
    spd=v(10,10),
    pos=level_index*128,
    draw_order=0,
    shk=v(0,0)
  }
)

function cam:update()
  self.pos.x=approach(self.pos.x,level_index.x*128,self.spd.x)
  self.pos.y=approach(self.pos.y,level_index.y*128,self.spd.y)

  if self.pos==level_index*128 then
    remove_old({"player","camera","key", "door"})
  end

  if shake > 0 then
    shk=v(rnd(1)<0.5 and 1 or -1,rnd(1)<0.5 and 1 or -1)
    shake=shake-1
  else
    shake=0
    shk=v(0,0)
  end

  if scene_player then
    local p=scene_player
    local l_ind=v(flr((p.pos.x+p.hitbox.xl+(p.hitbox.xr-p.hitbox.xl)/2)/128),
                  flr((p.pos.y+p.hitbox.yt+(p.hitbox.yb-p.hitbox.yt)/2)/128))

    if level_index ~= l_ind then
      level_index=l_ind
      load_level()
    end
  end

  if enable_light then
    for l in all(lights) do
      l.pos=v(self.pos.x,self.pos.y)+l.off
      l:update()
    end
  end
end

function cam:render()  
  camera(self.pos.x+shk.x,self.pos.y+shk.y)
end


-------------------------------
-- light
-------------------------------

light=object:extend({
  l1=3,l2=2
})

function light:init()
  self.llevel=0
end

function light:update()
  local llevel=0
  for e in all(lightpoints) do
    local dist=flr(#(v(e.pos.x-self.pos.x,e.pos.y-self.pos.y)/8))
    local r=self.l1
    local fall=self.l2
    if e.fr then r=e.fr end
    if e.ff then fall=e.ff end

    if dist < r*r then 
      llevel=2
    elseif dist < (r+fall)*(r+fall) then 
      if llevel~=2 then llevel=llevel+1 end
    end

  end

  self.llevel=llevel
end

function light:render()
  local p= 0 --self.llevel == 2 and 0b1111111111111111.1 or
           --(self.llevel == 1 and 0b1010010110100101.1 or 0b0000000000000000.1)

  fillp(p)
  rectfill(self.pos.x,self.pos.y,self.pos.x+7,self.pos.y+7,0)
  fillp()
end

-------------------------------
-- entity: light switch
-------------------------------

light_switch=entity:extend({})

light_switch:spawns_from(48)

function light_switch:init()
  enable_light=true
end

function light_switch:render()
  return
end

-------------------------------
-- entity: chest
-------------------------------

chest=entity:extend({
  collides_with={"player"},
  hitbox=box(0,0,8,8),
  draw_order=1
})

chest:spawns_from(5,6)

function chest:init()
  self.col=false
  self.obj=self.sprite==5 and key({pos=deep_copy(self.pos)}) or slowmo_obj({pos=deep_copy(self.pos+v(0,-4))})
  self.sprite=5
end

function chest:open()
  e_add(self.obj)

  for i=1,4 do
    e_add(smoke({
        pos=v(self.pos.x+rnd(8),self.pos.y+rnd(2))
        }))
  end
  shake=shake+2

  mset(self.map_pos.x,self.map_pos.y,0)
  self:become("nil")
end

function chest:update()
  if (not scene_player) then return end
  local ec,oc=c_get_entity({pos=self.pos,hitbox=box(-4,-4,12,12)}),c_get_entity(scene_player)
  self.col=ec.b:overlaps(oc.b)
  if btn(5) and ec.b:overlaps(oc.b) and self.state~="nil" then
    self.sprite=6
    self:become("open");
  end
end

function chest:render()
  spr(self.sprite,self.pos.x,self.pos.y)
  if self.col then
    rectfill(self.pos.x+1,self.pos.y-5,self.pos.x+4,self.pos.y-1,7)
    print("\151",self.pos.x,self.pos.y-5,0)
  end
end

function chest:collide(e)
  return c_push_out
end

-------------------------------
-- entity: key
-------------------------------

key=dynamic:extend({
  collides_with={"player","door"},
  hitbox=box(-4,-4,12,12),
  tags={"key"},
  maxvel=0.7,
  draw_order=2
})

key:spawns_from(16)

function key:init()
  self.render=spr_render
  self.sprite=16
end

function key:update()
  if not self.follows then return end

  self.dir=v(self.follows.pos.x-self.pos.x,
             self.follows.pos.y-self.pos.y)
  self.maxvel=sqrt(#self.dir)/15
  self.dir=self.dir:norm()

  self:set_vel()
end

function key:collide(e)
  if (e:is_a("player")) then self.follows=e end
  if e:is_a("door") then
    for i=1,4 do
      e_add(smoke({
        pos=v(self.pos.x+rnd(8),self.pos.y+rnd(8)),
        vel=v(rnd(0.5)-0.25,-(rnd(1)+0.5))
        }))
    end
    self.done=true
  end
end

-------------------------------
-- entity: door
-------------------------------

door=entity:extend({
  collides_with={"player","key"},
  tags={"door"},
  hitbox=box(0,0,16,16),
  keycount=3,
  draw_order=2
})

door:spawns_from(11)

function door:init()
	mset(self.map_pos.x,self.map_pos.y,0)	
end

function door:render()
  spr(self.sprite,self.pos.x,self.pos.y,2,2)
  if (self.keycount==3) then spr(13,self.pos.x-1,self.pos.y+5) end
  if (self.keycount<=3 and self.keycount>=1) then spr(13,self.pos.x+4,self.pos.y+5) end
  if (self.keycount<=3 and self.keycount>=2) then spr(13,self.pos.x+9,self.pos.y+5) end
end

function door:collide(e)
  if (e:is_a("player")) then return c_push_out end
  if e:is_a("key") then
    self.keycount=self.keycount-1
    if self.keycount<=0 then
      self.done=true 
      for i=1,8 do
        e_add(smoke({
          pos=v(self.pos.x+rnd(16),self.pos.y+rnd(16)),
          vel=v(rnd(0.5)-0.25,-(rnd(1)+0.5))
          }))
      end
      shake=shake+6
    end
    shake=shake+2
  end
end

-------------------------------
-- entity: slowmo
-------------------------------

slowmo_obj=entity:extend({
  collides_with={"player"},
  hitbox=box(0,0,8,8),
  sprite=49
})

slowmo_obj:spawns_from(49)

function slowmo_obj:init()
  self.orig=v(self.pos.x, self.pos.y)
end

function slowmo_obj:update()
  self.pos = self.orig+v(0, 2*sin(self.t/50))
end

function slowmo_obj:collide(e)
  slowmo=slowmo+150
  self.done=true
end

-------------------------------------------------------------------
-- particles
--    common class for all
--    particles
-------------------------------------------------------------------

particle=object:extend(
  {
    t=0,vel=v(0,0),
    lifetime=30
  }
)

 -- common initialization
 -- for all entity types
function particle:init()  
  if self.sprite then
   self.sprite=deep_copy(self.sprite)
   if not self.render then
    self.render=spr_render
   end
  end
end

-------------------------------
-- smoke particle
-------------------------------

smoke=particle:extend(
  {
    vel=v(0,0),
    c=7,
    v=0.1
  }
)

function smoke:init()
  self.vel=v(rnd(0.5)-0.25,-(rnd(1)+0.5))
  if not self.r then self.r=rnd(1)+1.5 end
end

function smoke:update()
  self.r=self.r-self.v
  if self.r<=0 then self.done=true end
end

function smoke:render()
  if (not self.pos) then return  end
  circfill(self.pos.x, self.pos.y, self.r, self.c)
end

-------------------------------
-- text particle
-------------------------------

ptext=particle:extend(
  {
    lifetime=20,
    txt="-1",
  }
)

function ptext:init()
  local vx=0
  if self.vh then vx=rnd(0.5)-0.5 end
  self.vel=v(vx,-(rnd(1)+0.5))
end

function ptext:update()
  if self.t > self.lifetime/3 then 
    self.vel=v(0,0) 
  end
end

function ptext:render()
  if (not self.pos) then return end

  --rectfill(self.pos.x,self.pos.y,self.pos.x+4*#self.txt+2,self.pos.y+4,0)

  print(self.txt,self.pos.x-1,self.pos.y,0)
  print(self.txt,self.pos.x+1,self.pos.y,0)
  print(self.txt,self.pos.x,self.pos.y-1,0)
  print(self.txt,self.pos.x,self.pos.y+1,0)
  print(self.txt,self.pos.x+1,self.pos.y+1,0)
  print(self.txt,self.pos.x+1,self.pos.y-1,0)
  print(self.txt,self.pos.x-1,self.pos.y+1,0)
  print(self.txt,self.pos.x-1,self.pos.y-1,0)
  print(self.txt,self.pos.x,self.pos.y,self.c or 7)

  if self.lifetime>0 and self.t > 2*self.lifetime/3 then
    draw_dithered(
      (self.lifetime-self.t)/(2*self.lifetime/3),false,
      box(self.pos.x,self.pos.y,self.pos.x+4*#self.txt+2,self.pos.y+4))
  end
end

-------------------------------
-- dialog box
-------------------------------

dialog=entity:extend({
  spd=1
  })

function dialog:init()
  self.text_obj=ptext({
    pos=v(self.pos.x,self.pos.y),
    txt="",
    vel=v(0,0),
    lifetime=-1,
    c=9
    })
  p_add(self.text_obj)
  self.text_obj.vel=v(0,0)
  self.text_obj.lifetime=-1
  self.index=0
  self.sline=15
  self.nline=1

  local i=self.sline
  while i<#self.text do
    while sub(self.text,i,i)~=" " and i<#self.text do
      i=i+1
    end
    if (i==#self.text)then break end
    self.text=sub(self.text,1,i) .. "\n" .. sub(self.text,i+1,#self.text)
    i=i+self.sline
  end
end

function dialog:update()
  if (not self.text) then return end

  if self.t > self.spd then 
    self.t = 0
    if self.index <= #self.text then
      self.index=self.index+1
      self.text_obj.txt=sub(self.text, 1, self.index)
      if (sub(self.text, self.index, self.index)=="\n") then self.nline=self.nline+1 end
    end
  end
  self.text_obj.pos=self.pos+v(-self.sline*2,-(self.nline-1)*5)
end
function dialog:destroy()
  self.text_obj.done=true
  self.done=true
end
function dialog:render() end

-------------------------------
-- collision system
-------------------------------

function do_movement()
  for e in all(entities) do
    if (slowmo>0 and (e_is_any(e,{"player","camera","attack"}) or slowmo_update)) or 
        (slowmo<=0) then
      if e.vel then
        e.pos.x=e.pos.x+e.vel.x
        collide_tile(e)        
        
        e.pos.y=e.pos.y+e.vel.y
        collide_tile(e)
      end
    end
  end
end

-------------
-- buckets
-------------

c_bucket = {}

function bkt_pos(e)
  local x,y=e.pos.x,e.pos.y
  return flr(shr(x,4)),flr(shr(y,4))
end

-- add entity to all the indexes
-- it belongs in the bucket
function bkt_insert(e)
  local x,y=bkt_pos(e)
  for t in all(e.tags) do
    local b=bkt_get(t,x,y)
    add(b,e)
  end

  e.bkt=v(x,y)
end

function bkt_remove(e)
  local x,y=e.bkt.x,e.bkt.y
  for t in all(e.tags) do
    local b=bkt_get(t,x,y)
    del(b,e)
  end
end

function bkt_get(t,x,y)
  local ind=t..":"..x..","..y
  if not c_bucket[ind] then
    c_bucket[ind]={}
  end
  return c_bucket[ind]
end

function bkt_update()  
  for e in all(entities) do
    bkt_update_entity(e)
  end
end

function bkt_update_entity(e)
  if not e.pos or not e.tags then return end
  local bx,by=bkt_pos(e)
  if not e.bkt or e.bkt.x~=bx or e.bkt.x~=by then
    if not e.bkt then
      bkt_insert(e)
    else
      bkt_remove(e)
      bkt_insert(e)
    end
  end
end

-- iterator that goes over
-- all entities with tag "tag"
-- that can potentially collide
-- with "e" - uses the bucket
-- structure described earlier.
function c_potentials(e,tag)
 local cx,cy=bkt_pos(e)
 local bx,by=cx-2,cy-1
 local bkt,nbkt,bi={},0,1
 return function()
  -- ran out of current bucket,
  -- find next non-empty one
  while bi>nbkt do
   bx=bx+1
   if (bx>cx+1) then  bx,by=cx-1,by+1 end
   if (by>cy+1) then return nil end
   bkt=bkt_get(tag,bx,by)
   nbkt,bi=#bkt,1
  end
  -- return next entity in
  -- current bucket and
  -- increment index
  local e=bkt[bi]
  bi=bi+1
  return e
 end 
end

function do_collisions()    
  	for e in all(entities) do
      collide(e)
    end
end

function collide(e)
  if not e.collides_with then return end
  if not e.hitbox then return end

  local ec=c_get_entity(e)

  ---------------------
  -- entity collision
  ---------------------
  for tag in all(e.collides_with) do
    --local bc=bkt_get(tag,e.bkt.x,e.bkt.y)
    for o in  c_potentials(e,tag) do  --all(entities[tag]) do
      -- create an object that holds the entity
      -- and the hitbox in the right position
      local oc=c_get_entity(o)
      -- call collide function on the entity
      -- that e collided with
      if o~=e and ec.b:overlaps(oc.b) then
        if ec.e.collide then 
          local func,arg=ec.e:collide(oc.e)
          if func then
            func(ec,oc,arg)            
          end
        end
      end

    end
  end
end


--------------------
-- tile collision
--------------------

function collide_tile(e)  
  -- do not collide if it's not set to
  if (not e.c_tile) then return end

  local ec=c_get_entity(e)

  local pos=tile_flag_at(ec.b, 1)

  for p in all(pos) do
    local oc={}
    oc.b=box(p.x,p.y,p.x+8,p.y+8)

    -- only allow pushing to empty spaces
    local dirs={v(-1,0),v(1,0),v(0,-1),v(0,1)}
    local allowed={}
    for i=1,4 do
      local np=v(p.x/8,p.y/8)+dirs[i]
      if np.x < 0 or np.x > 127 or np.y < 0 or np.y > 63 then
        allowed[i] = false
      else
        allowed[i]= not is_solid(np.x,np.y)
      end
    end

    if (ec.e.tcollide) then ec.e:tcollide() end
    c_push_out(oc, ec, allowed)
  end
end

-- get entity with the right position
-- for cboxes
function c_get_entity(e)
  local ec={}
  ec.e=e
  ec.b=e.hitbox--state_dependent(e,"hitbox")
  if (ec.b)then  ec.b=ec.b:translate(e.pos) end
  return ec
end

-- returns an entity's property 
-- depending on entity state
-- e.g. hitbox can be specified
-- as {hitbox=box(...)}
-- or {hitbox={
--  walking=box(...),
--  crouching=box(...)
-- }
function state_dependent(e,prop)
 local p=e[prop]
 if (not p) then return nil end
 if type(p)=="table" and p[e.state] then
  p=p[e.state]
 end
 if type(p)=="table" and p[1] then
  p=p[1]
 end
 return p
end

function tile_at(cel_x, cel_y)
	return mget(cel_x, cel_y)
end

function is_solid(cel_x,cel_y)
  return fget(mget(cel_x, cel_y),1)
end

function tile_flag_at(b, flag)
  local pos={}

	for i=flr(b.xl/8), ((ceil(b.xr)-1)/8) do
		for j=flr(b.yt/8), ((ceil(b.yb)-1)/8) do
			if(fget(tile_at(i, j), flag)) then
				add(pos,{x=i*8,y=j*8})
			end
		end
	end

  return pos
end

-- reaction function, used by
-- returning it from :collide().
-- cause the other object to
-- be pushed out so it no
-- longer collides.
function c_push_out(oc,ec,allowed_dirs)
 local sepv=ec.b:sepv(oc.b,allowed_dirs)
 if not sepv then return end
 ec.e.pos=ec.e.pos+sepv
 if ec.e.vel then
  local vdot=ec.e.vel:dot(sepv)
  if vdot<0 then   
   if sepv.x~=0 then ec.e.vel.x=0 end
   if sepv.y~=0 then ec.e.vel.y=0 end
  end
 end
 ec.b=ec.b:translate(sepv)
 end
-- inverse of c_push_out - moves
-- the object with the :collide()
-- method out of the other object.
function c_move_out(oc,ec,allowed)
 return c_push_out(ec,oc,allowed)
end

function c_push_vel(oc,ec,args)
  if (not args) then args={} end
  local sepv=ec.b:sepv(oc.b,args[1])  
  if not sepv then return end
  if #sepv>0.2 then sepv=sepv:norm() end
  if not ec.e.vel then return end
  if (args[2]) then sepv=sepv*args[2] end
  ec.e.vel=sepv
end
-- inverse of c_push_out - moves
-- the object with the :collide()
-- method out of the other object.
function c_move_out(oc,ec,allowed)
 return c_push_out(ec,oc,allowed)
end


--------------------
-- entity handling
--------------------

entities = {}
particles = {}
old_ent = {}

function p_add(p)  
  add(particles, p)
end

function p_remove(p)
  del(particles, p)
end

function p_update()
  for p in all(particles) do
    if p.pos and p.vel then
      p.pos=p.pos+p.vel
    end
    if (p.update) then p:update() end

    if (p.lifetime>0 and p.t > p.lifetime) or p.done then
      p_remove(p)
    else
      p.t=p.t+1
    end
  end
end

-- adds entity to all entries
-- of the table indexed by it's tags
function e_add(e)
  add(entities, e)

  if e.draw_order then
    if (not r_entities[e.draw_order]) then  r_entities[e.draw_order]={} end
    add(r_entities[e.draw_order],e)
  else
    if (not r_entities[3]) then r_entities[3]={} end
    add(r_entities[3],e)
  end
end

function e_remove(e)
  del(entities, e)
  for tag in all(e.tags) do        
    if e.bkt then
      del(bkt_get(tag, e.bkt.x, e.bkt.y), e)
    end
  end

  if e.draw_order then    
    del(r_entities[e.draw_order],e)
  else  
    del(r_entities[3],e)
  end

  if e.destroy then e:destroy() end
end

-- loop through all entities and
-- update them based on their state
function e_update_all()  
  for e in all(entities) do
    if (slowmo>0 and (e_is_any(e,{"player","camera","attack"}) or slowmo_update)) or 
        (slowmo<=0) then
      if e[e.state] then
        e[e.state](e)
      end
      if e.update then
        e:update()
      end
      e.t=e.t+1

      if e.done then
        e_remove(e)
      end
    end
  end  
end

r_entities = {}

function e_draw_all()
  for i=0,7 do
    for e in all(r_entities[i]) do
      if debug then
        local ec=c_get_entity(e)
        if ec.b then
          rectfill(ec.b.xl,ec.b.yt,ec.b.xr,ec.b.yb,8)
        end
      end

      e:render()
    end
  end
end

function p_draw_all()
  for p in all(particles) do
    p:render()
  end
end

function spr_render(e)
  spr(e.sprite, e.pos.x, e.pos.y)
end

-------------------------------
-- helper functions
-------------------------------

function sign(val)
  return val<0 and -1 or (val > 0 and 1 or 0)
end

function frac(val)
  return val-flr(val)
end

function ceil(val)
  if (frac(val)>0) then return flr(val+sign(val)*1) end
  return val
end

function approach(val,target,step)
  step=abs(step)
  if val < target then
    return min(val+step,target)
  else
    return max(val-step,target)
  end  
end

function remove_old(tags)
  for e in all(old_ent) do
    local rmv=true
    for t in all(tags) do
      if (e.is_a and e:is_a(t))then  rmv=false end
    end

    if (rmv) then e_remove(e) end
  end
  old_ent={}
end

function draw_dithered(t,flip,box,c)
  local low,mid,hi=0, --0b0000000000000000.1,
                   0, --0b1010010110100101.1,
                   0 --0b1111111111111111.1                
  if flip then low,hi=hi,low end

  if t <= 0.3 then
    fillp(low)
  elseif t <= 0.6 then
    fillp(mid)
  elseif t <= 1 then
    fillp(hi)
  end

  if box then
    rectfill(box.xl,box.yt,box.xr,box.yb,c or 0)
    fillp()
  end
end

function add_time(t)
  global_timer.t=global_timer.t+t
end

function timer_update(timer)
  timer.t= timer.t-time()-last
end

function e_is_any(e, op)
  if not e.is_a then return end
  for i in all(e.tags) do
    for o in all(op) do
      if e:is_a(o) then return true end
    end
  end

  return false
end

-------------------------------
-- init, update, draw
-------------------------------

global_timer={ t = 60 }
lights={}
enable_light=false
slowmo = 0
slowmo_update=false
lightpoints={}

function _init()
  global_timer={ t = 60 }
  load_level()
  e_add(cam(
    {

    }
  ))

  last=time()

  for i=0,15 do
    for j=0,15 do
      add(lights, light({off=v(i,j)*8,pos=v(i,j)*8}))
    end
  end
end

function _update()
  e_update_all()
  bkt_update()
  do_movement()
  do_collisions()
  p_update()

  if slowmo<=0 or (slowmo>0 and slowmo_update) then
    timer_update(global_timer)
  end

  if slowmo>0 then slowmo=slowmo-1 end

  slowmo_update=not slowmo_update
  last=time()

  --if btnp(4) then slowmo=not slowmo end
end

function _draw()
  cls()

  slowmo=1
  if slowmo>0 then
    pal(0,7)
    pal(7,0)
    pal(9,0)
  end
  rectfill(0,0,128,128,0)
  palt(0, false)
  palt(1, true)
  e_draw_all()
  p_draw_all()
  palt()
  pal()

  if enable_light then
    for l in all(lights) do
      l:render()
    end
  end

  camera()

  rectfill(1,1,13,11,0)
  rect(1,1,13,11,7)
  print(flr(global_timer.t),4,4,7)

  if debug then
  local cpu,mem=flr(100*stat(1)),flr(stat(0))
  print("cpu: " .. cpu .. " mem: " .. mem .. " ent: " .. #entities, 1, 0, 0)
  print("cpu: " .. cpu .. " mem: " .. mem .. " ent: " .. #entities, 2, 1, 0)
  print("cpu: " .. cpu .. " mem: " .. mem .. " ent: " .. #entities, 0, 1, 0)
  print("cpu: " .. cpu .. " mem: " .. mem .. " ent: " .. #entities, 1, 2, 0)
  print("cpu: " .. cpu .. " mem: " .. mem .. " ent: " .. #entities, 1, 1, 14)  
  end
end
__gfx__
11000001111111111100000111111111110700110000000007777770111111111111111111111111007777000000000000000000111111110000000000000000
10077701110000011007770110001111110770110777777070000007111111111111111111111111070000700000007777000000110000110000000000000000
10777701100777011077770100700000110770117000000770000007111111111111111111000011700000070000777007770000100770010000000000000000
10770701107777011077070177777777110770117007700700000000100000011111111110099001707007070007007007007000107007010000000000000000
10777700107707011077770170777770100770017770077777700777009999000000000000900900700000070077007007007700100700010000000000000000
10000077107777001000000000700000107777017007700770077007090000900999999009000090700000070707007007007070110770110000000000000000
17777007177770771777707710001111100700017000000770000007090000909000000909099090070000707007007007007007110000110000000000000000
17007007170070071700700711111111110770117777777777777777009999000999999000900900007777007007007007007007111111110000000000000000
11111111007777000000000099999000000000000000000000000000000000000000000000000000000000007007007007007007000000000000000000000000
11111111070000700000000000909099999990900000000000000000000000000000000000000000000000007007007007007007000000000000000000000000
00000111007777000077700099999009009090990000000000000000000000000000000000000000000000007007007007007007000000000000000000000000
07770000070000700700007000099099999990090000000000000000000000000000000000000000000000007007007007007007000000000000000000000000
07077777777777777700777700009090000990990000000000000000000000000000000000000000000000007007007007007007000000000000000000000000
07770707777777777777077799999099999990900000000000000000000000000000000000000000000000007007007007007007000000000000000000000000
00000000077777700077777090000009900000990000000000000000000000000000000000000000000000007007007007007007000000000000000000000000
11111111007777000000000099999999999999990000000000000000000000000000000000000000000000007777777777777777000000000000000000000000
11000001100777011100000110077701110000011111111111111111111111111111111100000000000000000000000000000000000000000000000000000000
10077701107070011007770110777001100777011100000111100000111000011111111100000000000000000000000000000000000000000000000000000000
10770701107777011077070110777701107707011007770111007770000077001000000000000000000000000000000000000000000000000000000000000000
10777701100000011077770110000001107777011077070111077070077077700077077000000000000000000000000000000000000000000000000000000000
10000001007777001000000100777700100000011077770111077770007070700007077700000000000000000000000000000000000000000000000000000000
10777701070000701077770107000070107777011000000100000000007077700007077700000000000000000000000000000000000000000000000000000000
10700701000110001007700100011000100770011077770100777701077000000077077700000000000000000000000000000000000000000000000000000000
10000001111111111100001111111111110000111000000100000001000011110000000000000000000000000000000000000000000000000000000000000000
000aa000007777000000000011111111111111111000100011111111110707010077000000000000000000000000000000000000000000000000000000000000
00a00a00070000700007770000000111111111110070007011100011100797007700770000000000000000000000000000000000000000000000000000000000
0a0000a0700700070070007099990011100001110700900710009000107707700000000000000000000000000000000000000000000000000000000000000000
0a0000a0700700070007770090909001009900010779097700790970100090000077007700000000000000000000000000000000000000000000000000000000
00a00a00700070070070007090090900090999000700900707709077111000110000770000000000000000000000000000000000000000000000000000000000
00066000700007070777777709907770900977700000000000700070111111117700000000000000000000000000000000000000000000000000000000000000
00077000070000700077777007000007999000071111111110000000111111110077700700000000000000000000000000000000000000000000000000000000
00066000007777000007770000777770007777701111111111111111111111110000000000000000000000000000000000000000000000000000000000000000
00000000007700000077000000077000000077777700077770000000077777700000000000000000000000000000000000077007700000000000000000000000
07007000070770707707700000700700077700000077700007770077070000707777777777777777077777777777777000077770777000000000000000000000
70700007707707700770000000777700700000000000000000007707070000707000000000000007070000000000007000077007000770000000000000000000
07000000770000000000000000707700700000000000000000000007070000707000000000000007070000000000007007777000700007700000000000000000
00000700000007000000770000707700070000000000000000000007070000707000000000000007070000000000007007070007000000700000000000000000
00007070077070700007700700770700070000000000000000000070070000707000000000000007070000000000007007007000700000700000000000000000
07000700707077700000707000770700070000000000000000000070070000707777777777777777070000777700007007000007000000700000000000000000
00000000770007700000000000777700070000000000000000000070070000700700070007000700070000700700007007000007700000700000000000000000
00000000077077700777000000777700070000000000000000000070070000700000000007000070070000700700007007000770077000700000000000000000
00000700777077707777700000707700700000000000000000000070070000707777777707000070070000777700007007077000000770700000000000000000
00000000770077000777000000777700700000000000000000000007070000700000000007000070070000000000007007700000000007700000000000000000
00700000000777000000000000770700700000000000000000000007070000700000000007000070070000000000007000707770077707000000000000000000
00000000770000000000777000770700700000000000000000000007070000700000000007000070070000000000007000707070070707000000000000000000
00000000777077700007777700770700070000000000000000000070070000700000000007000070070000000000007000707070077707000000000000000000
70007700777077000000777000777700070000000000000000000070070000707777777707000070077777777777777000707070000007000000000000000000
70000000000000000000000000777700070000000000000000000070077777700700070007000070070007000700007000777777777777000000000000000000
00000000000000000770077000777700700000000000000000000007070070700700070000000000070007000700007000000000000000000000000000000000
00000077770000007000700700707070700000000000000000000007077777707777777700000000077777777777777000007000000000000000000000000000
00077700007770007007000700777070700000000000000000000007070700700070007007770000070000700070007000000000000000000000000000000000
00770707007077000070007000707070070000000000000000000070077777700070007007077777070000700070007000000070000000000000000000000000
07007000000700700770070000707700070000000000000000000070070070707777777707770707077777777777777000000000000000000000000000000000
70700070070007077007700700777700007000000000000000000007077777707000700000000000070070007000707000007000000000000000000000000000
77007700007700777007000707777770000777000007777700007770070707007000700000000000070070007000707000000000000000000000000000000000
70007000000700700770077077707077000000777770000077770000007070707777777700000000077777777777777000000070000000000000000000000000
07000070700000701111111100000000070007000770770000000000000000000000000000000000000000000000000000000000000000000000000000000000
70707007000707071111111107000700077077007770077000000900000000000000000000000000000000000000000000000000000000000000000000000000
77070700707070770000000107000700077077007707707009009090000000000000000000000000000000000000000000000000000000000000000000000000
00777707707777000770070100000000000000000077000090900900000000000000000000000000000000000000000000000000000000000000000000000000
00070077770070007007770100000000007000707070077009000000000000000000000000000000000000000000000000000000000000000000000000000000
00700700007007000700070100700070007707707700777000000700000000000000000000000000000000000000000000000000000000000000000000000000
07077070070770700077700100700070007707700770770007000000000000000000000000000000000000000000000000000000000000000000000000000000
07770007700077701000001100000000000000000000000007000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000060000000000
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
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000003434340000340000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000340000003434000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000034000034000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000003400340000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000340034340000000000343434000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000340000000000003434003400000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000003434340000000000343400340000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000003434003434000034343434343400000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000340034340034343434003434000034000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000034343434343400343434340000000034000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000034343434343434343434340000343434000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000003434000034343434343434340034343400000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000003400000034343434343400343434340000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000003400000000003434343434003434000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000003434343434343400000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000034340000003400000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000343400340000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
67450000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
__label__
00000000000000000077770000777700007777000077770000000000000000000077770000777700000000000000000000777700007777000077770000000000
07777777777777000070770000707700007077000070770000000077770000000070770000707700000000777700000000707700007077000070770000000077
07000000000007000077770000777700007777000077770000077700007770000077770000777700000777000077700000777700007777000077770000077700
07000000000007000077070000770700007707000077070000770707007077000077070000770700007707070070770000770700007707000077070000770707
07007070707007700077070000770700007707000077070007007000000700700077070000770700070070000007007000770700007707000077070007007000
77007070707007070077070000770700007707000077070070700070070007070077070000770700707000700700070700770700007707000077070070700070
77007770777007770077770000777700007777000077770077007700007700770077770000777700770077000077007700777700007777000077770077007700
77000070007007700077770000777700007777000077770070007000000700700077770000777700700070000007007000777700007777000077770070007000
07000070007007700077770000777700007777000077770007000070700000700077770000777700070000707000007000777700007777000077770007000070
77000000000007070070707000707070007077000070707070707007000707070070770000707700707070070007070700707070007077000070707070707007
77000000000007770077707000777070007777000077707077070700707070770077770000777700770707007070707700777070007777000077707077070700
07777777777777000070707000707070007707000070707000777707707777000077070000777700007777077077770000707070007707000070707000777707
00070077770070000070770000707700007707000070770000070077770070000077070000770700000700777700700000707700007707000070770000070077
00700700007007000077770000777700007707000077770000700700007007000077070000770700007007000070070000777700007707000077770000700700
07077070070770700777777007777770007777000777777007077070070770700077770000777700070770700707707007777770007777000777777007077070
07770007700077707770707777707077007777007770707707770007700077700077770000777700077700077000777077707077007777007770707707770007
00000000000000000000000000770000007777000000000000000000000000000077770000777700000000000000000000000000007777000000000000000000
00000077770000000000000007077070007070700000000000000000000000000070707000707070000000000000000000000000007070700000000000000000
00077700007770000000000070770770007770700000000000000000000000000077707000777070000000000000000000000000007770700000000000000000
00770707007077000000000077000000007070700000000000000000000000000070707000707070000000000000000000000000007070700000000000000000
07007000000700700000000000000700007077000000000000000000000000000070770000707700000000000000000000000000007077000000000000000000
70700070070007070000000007707070007777000000000000000000000000000077770000777700000000000000000000000000007777000000000000000000
77007700007700770000000070707770077777700000000000000000000000000777777007777770000000000000000000000000077777700000000000000000
70007000000700700000000077000770777070770000000000000000000000007770707777707077000000000000000000000000777070770000000000000000
07000070700000700000000000000000000000000000000000000000000000000000000000000700000000000000000000000000000000000000000000000000
70707007000707070000000000000000000000000000000000000000000000000000000000007770000000000000000000000000000000000000070000000000
77070700707070770000000000000000000000000000000000000000000000000000000000000700000000000000000000000000000000000000000000000000
00777707707777000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000070000000000000
00070077770070000000000000000000000000000000000000000000000000000000000000070000000000000000000000000000000000000000000000000000
00700700007007000000000000000000000000000000000000000000000000000000000000777000000000000000000000000000000000000000000000000000
07077070070770700000000000000000000000000000077700000000000000000000000000070000000000000000000000000000000000007000770000000000
07770007700077700000000000000000000000000000070000000000000000000000000000000000000000000000000000000000000000007000000000000000
00000000000000000000000000000000000000000000070000000000000000000000000000077007700000000000000700000000000000000000000000000000
00000077770000000000000000000000000000000007700000000000000000000000000000077770777000000000000000000000000000000000000000000000
00077700007770000000000000000000000000000007707770000000000000000000000000077007000770000000000000000000000000000000000000000000
00770707007077000000000000000000000000000000007000000000000000000000000007777000700007700070000000000000000000000000000000000000
07007000000700700000000000000000000000000000007000000000000000000000000007070007000000700000000000000000000000000000000000000000
70700070070007070000000000000000000000000000777000000000000000000000000007007000700000700000090070000000000000000000000000000000
77007700007700770000000000000000000000000000777000000000000000000000000007000007000000700000009000000000000000000000000000000000
70007000000700700000000000000000000000000000000000000000000000000000000007000007700000700000099900000000000000000000000000000000
07000070700000700077000000000000000000000000000000000000000000000000000007000770077000700000009000000000000000000000000000000000
70707007000707070707707000000000000000000000000000000000000000000000000007077000000770700900000000000000000000000000000000000000
77070700707070777077077000000000000000000000000000000000000000000000000007700000000007700000000000000000000000000000000000000000
00777707707777007700000000000000000000000000000007700700000000000000000000707770077707000007000000000000000000000000000000000000
00070077770070000000070000000000000000000000000070077700000000000000000000707070070707000007900000000000000000000000000000000000
00700700007007000770707000000000000000000000000007000700000000000000000000707070077707000000900000000000000000000000000000000000
07077070070770707070777000000000000000000000000000777000000000000000000000707070000007000009990000000000000000000000000000000000
07770007700077707700077000000000000000000000000000000000000000000000000000777777777777000000900000000000000000000000000000000000
00000000000000000000000000000000000000000000000000077000000000000000000000000000000000000007000000000000000000000000000000000000
00000077770000000000000000000000000000000000000000700700000000000000000000000000000000000077700000000000000000000000000000000000
00077700007770000000000000000000000000000000000000777700000000000000000000000000000000000097900000000000000000000000000000000000
00770707007077000000000000000000000000000000000000707700000000000000000000000000000000000099900700000000000000000000000000000000
07007000000700700000000000000000000000000000000000707700000000000000000000000000000000000999990000000000000000000000000000000000
70700070070007070000000000000000000000000000000000770700000000000000000000000000000000000999990000000000000000000000000000000000
77007700007700770000000000000000000000000000000000770700000000000000000000000000000000000999990000000000000000000000000000000000
70007000000700700000000000000000000000000000000000777700000000000000000000000000000000000797770000000000000000000000000000000000
07000070700000700000000000000000000000000000000000777700000000000000000000000000000000000777777000000000000000000000000000000000
70707007000707070000000000000000070007000000000000707700000000000000000000077700000000007077777700000000000000000000000000000000
77070700707070770000000000000000070007000000000000777700000000000000000000777700000000007077777700000000000000000000000000000000
00777707707777000000000000000000000000000000000000770700000000000000000000770700000000000077777000000000000000000000000000000000
00070077770070000000000000000000000000000000000000770700000000000000000000777700000000000770070000000000000000000000000000000000
00700700007007000000000000000000007000700000000000770700000000000000000000000000000000007007700700000000000000000000000000000000
07077070070770700000000000000000007000700000000000777700000000000000000007777077000000007007000700000000000000000000000000000000
07770007700077700000000000000000000000000000000000777700000000000000000007007007000000000770077000000000000000000000000000000000
00000000000000000077000000000000000000000000000000777700000000000000000000000000000000000000000000000000000000000000000000000000
00000077770000000707707000000000000000000000000000707070000000000000000000000000000000000000000000000000000000000000000000000000
00077700007770007077077000000000000000000000000000777070000000000000000000000000000000000000000000000000000000000000000000000000
00770707007077007700000000000000000000000000000000707070000000000000000000000000000000000000000000000000000000000000000000000000
07007000000700700000070000000000000000000000000000707700000000000000000000000000000000000000000000000000000000000000000000000000
70700070070007070770707000000000000000000000000000777700000000000000000000000000000000000000000000000000000000000000000000000000
77007700007700777070777000000000000000000000000007777770000000000000000000000000000000000000000000000000000000000000000000000000
70007000000700707700077000000000000000000000000077707077000000000000000000000000000000000000000000000000000000000000000000000000
07000070700000700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
70707007000707070000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
77070700707070770000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00777707707777000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00070077770070000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00700700007007000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
07077070070770700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
07770007700077700000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000077000000770000000000000000000000000000000000000000000000000000000000000000000000000000
00000077770000000000000000000000000000007700770077007700000000000000000000000000000000000000000000000000000000000000000000000000
00077700007770000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00770707007077000000000000000000000000000077007700770077000000000000000000000000000000000000000000000000000000000000000000000000
07007000000700700000000000000000000000000000770000007700000000000000000000000000000000000000000000000000000000000000000000000000
70700070070007070000000000000000000000007700000077000000000000000000000000000000000000000000000000000000000000000000000000000000
77007700007700770000000000000000000000000077700700777007000000000000000000000000000000000000000000000000000000000000000000000000
70007000000700700000000000000000000000000000000000000000000000000000000000000077700000000000000000000000000000000000000000000000
07000070700000700000000000770000000000000077000000770000000000000000000000000770700000000000000000000000000000000000000000000000
70707007000707070000000007077070000000007700770077007700000000000700700000000777700000000000000000000000000000000000070000000000
77070700707070770000000070770770000000000000000000000000000000007070000700000000000000000000000000000000000000000000000000000000
00777707707777000000000077000000000000000077007700770077000000000700000000000777700000000000000000000000000000000070000000000000
00070077770070000000000000000700000000000000770000007700000000000000070000000700700000000000000000000000000000000000000000000000
00700700007007000000000007707070000000007700000077000000000000000000707000000000000000000000000000000000000000000000000000000000
07077070070770700000000070707770000000000077700700777007000000000700070000000000000000000000000000000000000000007000770000000000
07770007700077700000000077000770000000000000000000000000000000000000000000000000000000000000000000000000000000007000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000077000000000000000000000000000000000000
00000077770000000000000000000000000000000000000000000000000000000000000000000000000000000707707000000000000000000000000000000000
00077700007770000000000000000000000000000000000000000000000000000000000000000000000000007077077000000000000000000000000000000000
00770707007077000000000000000000000000000000000000000000000000000000000000000000000000007700000000000000000000000000000000000000
07007000000700700000000000000000000000000000000000000000000000000000000000000000000000000000070000000000000000000000000000000000
70700070070007070000000000000000000000000000000000000000000000000000000000000000000000000770707000000000000000000000000000000000
77007700007700770000000000000000000000000000000000000000000000000000000000000000000000007070777000000000000000000000000000000000
70007000000700700000000000000000000000000000000000000000000000000000000000000000000000007700077000000000000000000000000000000000
07000070700000700077000000000000000770000077000000000000000000000077000000000000000770000000000000000000000000000000000000000000
70707007000707070707707000000000007007000707707000000000000000000707707000000000007007000000000000000000000000000000000000000000
77070700707070777077077000000000007777007077077000000000000000007077077000000000007777000000000000000000000000000000000000000000
00777707707777007700000000000000007077007700000000000000000000007700000000000000007077000000000000000000000000000000000000000000
00070077770070000000070000000000007077000000070000000000000000000000070000000000007077000000000000000000000000000000000000000000
00700700007007000770707000000000007707000770707000000000000000000770707000000000007707000000000000000000000000000000000000000000
07077070070770707070777000000000007707007070777000000000000000007070777000000000007707000000000000000000000000000000000000000000
07770007700077707700077000000000007777007700077000000000000000007700077000000000007777000000000000000000000000000000000000000000
00000000000000000000000000000000007777000007700000000000000000000007700000077000007777000000000000000000000000000000000000000000
00000077770000000000007777000000007077000070070000000077770000000070070000700700007077000000007777000000000000777700000000000077
00077700007770000007770000777000007777000077770000077700007770000077770000777700007777000007770000777000000777000077700000077700
00770707007077000077070700707700007707000070770000770707007077000070770000707700007707000077070700707700007707070070770000770707
07007000000700700700700000070070007707000070770007007000000700700070770000707700007707000700700000070070070070000007007007007000
70700070070007077070007007000707007707000077070070700070070007070077070000770700007707007070007007000707707000700700070770700070
77007700007700777700770000770077007777000077070077007700007700770077070000770700007777007700770000770077770077000077007777007700
70007000000700707000700000070070007777000077770070007000000700700077770000777700007777007000700000070070700070000007007070007000
07000070700000700700007070000070007777000077770007000070700000700077770000777700007777000700007070000070070000707000007007000070
70707007000707077070700700070707007077000070770070707007000707070070770000707700007077007070700700070707707070070007070770707007
77070700707070777707070070707077007777000077770077070700707070770077770000777700007777007707070070707077770707007070707777070700
00777707707777000077770770777700007707000077070000777707707777000077070000770700007707000077770770777700007777077077770000777707
00070077770070000007007777007000007707000077070000070077770070000077070000770700007707000007007777007000000700777700700000070077
00700700007007000070070000700700007707000077070000700700007007000077070000770700007707000070070000700700007007000070070000700700
07077070070770700707707007077070007777000077770007077070070770700077770000777700007777000707707007077070070770700707707007077070
07770007700077700777000770007770007777000077770007770007700077700077770000777700007777000777000770007770077700077000777007770007

__gff__
0000000000000000000000000002020200000000000000000000000000020202000000000000000000000000000200020000000000000000000000000000000001030103010301030303030303030202010301030300030303030303030302020303020301030103010001010100020203030000000301000000000000000002
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
__map__
0000000000000000000000000000606160615360615040755075436061437553436061005000004360614043606140634a58585858585858585858585858584b00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000707170715370714360616061537071534363537071606160615370710053707160615968686868686868686868686868685900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000006300005370717071530000635375630000707170715300004363000070715711000000000000000000000000115900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000005300000000630000006375606100000000006300005300000000416700000000000000747407000000005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000006061000000006300000000000000000075707100000000000000006300000000520000007474007400000000000000005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000007071000000000000000000000000000000000000000000000000000000000000414700000000007400000000000000005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000040000000004100500000000000000000000040435974740074000000137474747474005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000006061000000000050000000000000000041410000000000000000000000006061535900000074007474007413000074005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000070710000000000000000000000000000000000000000000000000000000070716359007400000000000a7400040074005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000606100000000000000000000000000000000000000000000000000000060615900740074740074007413000074005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000707100000000000000005000000041606100000000400000000050000070715900000000000074137474747474005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000004000000000000000000041707100000000000000000000000060615974740000740000000000000000005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000070715900000000740000000000000000005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000060610000500000000000000000000040000000000041415900000000070000000000000000005900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000043000000004300000070710000006061434360610000000000000000410041415911000000000000000000000000115900000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000053606160615343000000000000007071536370710000000000000000004341415a58585858585858585858585858585b00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000000000006370717071535300000000000000000053606143000050000000000000530000606141434175434360616061434300415a5858585858490b0c4858585858585b000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000063530000000000000000005370715360610040606100064353606170714163606153537071707153536061436868686868681b1c68686868684343000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000053000000000000000000530000637071414170716061536370716061000070716353000000005363707153430000000000000000000000005353000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000063000000000000000000630000370000000000007071630050417071000000000063000000006300000063530000000000000000000040435363000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000000000000003700000060614150004000000000000072000000004100630000000100000000500000536300000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000000500000000000000070714141000000000000000043000000004143000000000000000000000000630043000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000500063000000000063410000000000000000000000000053000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000011731100000000000000000000070000000000000000000000000000000050000000006220000000000063000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000073137300000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000500011731100000000000000000000000000000000000000000000000040000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000040000000000000000000000000400000000000000000000000004c4d00000000767600000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000005000070000000000004075754100000000005c5d00000076606100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000430000000000000000000000000000000000000000007676417541757575750000424200000076707100010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000436061530000400043000060610000000000000000000000437676757573137375410000014200000000767600000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000537071537500000053606170716061004360610060616061536061417573730011110000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000635050637500004153707140507071435370714170717071537071757573737375417541417541757541754175414175000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
