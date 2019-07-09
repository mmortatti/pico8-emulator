﻿
__lua__
local worldmap = {
	{7, 109, 13, 109, 13, 109, 13, 7},
	{67, 0, 0, 0, 0, 0, 0, 99},
	{65, 0, 0, 0, 0, 0, 0, 103},
	{67, 0, 0, 0, 0, 0, 0, 99},
	{65, 0, 0, 0, 0, 0, 0, 103},
	{67, 0, 0, 0, 0, 0, 0, 99},
	{65, 0, 0, 101, 101, 0, 0, 103},
	{7, 1, 1, 1, 1, 1, 1, 7}
}

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

local posx,posy=4,4
local dirx,diry=-1,0
local planex,planey=0,0.5
local wallheight=128
local texwidth,texheight=16,16

local objects={{pos=v(2,4),dir=v(-1,0)}}



function ray_plane_intersect(n,p0,l0,l)
  local d=n.dot(l)
  if d>=0.001 then
    local p0l0=p0-l0
    return p0l0.dot(n)>=0
  end
  return false
end

function line_line_intersection(a,b,c,d)
  local a1=b.y-a.y
  local b1=a.x-b.x
  local c1=a1*a.x-b1*a.y

  local a2=d.y-c.y
  local b2=c.x-d.x
  local c2=a2*c.x-b2*c.y

  local det=a1*b2-a2*b1
  if abs(det) <= 0.01 then
    return false
  else
    local x=(b2*c1-b1*c2)/det
    local y=(a1*c2-a2*c1)/det

  end
end

function render()
  for x=0,127 do
    local camx=2*x/127 - 1
    local rayx=dirx+planex*camx
    local rayy=diry+planey*camx
    
    local mapx,mapy=flr(posx),
                    flr(posy)
    local sidedistx,sidedisty=0,0
    
    local ddistx=abs(1/rayx)
    local ddisty=abs(1/rayy)
    
    local perpwalldist=0
    
    local stepx,stepy=0,0
    local hit=false
    local hittype=0
    local side=0
    
    if rayx < 0 then
      stepx=-1
      sidedistx=(posx-mapx)*ddistx
    else
      stepx=1
      sidedistx=(mapx+1-posx)*ddistx
    end
    
    if rayy<0 then
      stepy=-1
      sidedisty=(posy-mapy)*ddisty
    else
      stepy=1
      sidedisty=(mapy+1-posy)*ddisty
    end
    
    while not hit do
      if sidedistx < sidedisty then
        sidedistx=sidedistx+ddistx
        mapx=mapx+stepx
        side=0
      else
        sidedisty=sidedisty+ddisty
        mapy=mapy+stepy
        side=1
      end
      
      if worldmap[mapy][mapx]>0 then 
        hit=1
        hittype=1
      elseif get_obj_in_pos(mapx,mapy)~=nil then
        hit=1
        hittype=2
      end
    end
    
    if side==0 then
      perpwalldist=(mapx-posx+(1-stepx)/2)/rayx
    else
      perpwalldist=(mapy-posy+(1-stepy)/2)/rayy
    end
    
    
    local lineheight=flr(wallheight/perpwalldist)
    local drawstart=-lineheight/2+wallheight/2
    local drawend=lineheight/2+wallheight/2

    local texnum
    if hittype==1 then
      texnum=worldmap[mapy][mapx]-1
    else
      texnum=64
    end

    local wallx=0
    if side==0 then wallx=posy+perpwalldist*rayy
    else wallx=posx+perpwalldist*rayx end
    wallx=wallx-flr(wallx)

    local texx=flr(wallx*texwidth)
    if side==0 and rayx>0 then texx=texwidth-texx-1 end
    if side==1 and rayy<0 then texx=texwidth-texx-1 end

    local d = drawstart - wallheight/2 + lineheight/2
    local texy_start = ((d * texheight) / lineheight)
    d = drawend - wallheight/2 + lineheight/2
    local texy_end = ((d * texheight) / lineheight)

    local nty=flr(texnum/16)
    local ntx=texnum-nty*16
	line(x,drawstart,x,drawend, 7)
    --sspr(texx+ntx*8,texy_start+nty*8,1,texy_end-texy_start,x,drawstart,1,drawend-drawstart)

    line(x,drawend,x,128,13)
  end
end

function get_obj_in_pos(x,y)
  for obj in all(objects) do
    if flr(obj.pos.x)==x and flr(obj.pos.y)==y then
      return obj
    end
  end
  return nil
end

function _draw()
  cls()
  rectfill(0,112,128,128,1)
  render()
end
function _update()
	local mvspd=0.5
	local rotspd=0.01
	if btn(2) then
		if worldmap[flr(posy)][flr(posx+dirx*mvspd)] == 0 then
		 posx=posx+dirx*mvspd
		end
		if worldmap[flr(posy+diry*mvspd)][flr(posx)] == 0 then
		 posy=posy+diry*mvspd
		end
	end
	
	if btn(3) then
		if worldmap[flr(posy)][flr(posx-dirx*mvspd)] == 0 then
		 posx=posx-dirx*mvspd
		end
		if worldmap[flr(posy-diry*mvspd)][flr(posx)] == 0 then
		 posy=posy-diry*mvspd
		end
	end
	
	if btn(0) then
		local odx,opx=dirx,planex
		dirx=dirx*cos(-rotspd)-diry*sin(-rotspd)
		diry=odx*sin(-rotspd)+diry*cos(-rotspd)
		planex=planex*cos(-rotspd)-planey*sin(-rotspd)
		planey=opx*sin(-rotspd)+planey*cos(-rotspd)
	end
	
	if btn(1) then
	 local odx,opx=dirx,planex
		dirx=dirx*cos(rotspd)-diry*sin(rotspd)
		diry=odx*sin(rotspd)+diry*cos(rotspd)
		planex=planex*cos(rotspd)-planey*sin(rotspd)
		planey=opx*sin(rotspd)+planey*cos(rotspd)
	end
end

__gfx__
77777000007777777777700000777777777777000007777777777777777777777000000000000007000000000000000000000000000000000000000000000000
77770bffbb07777777770bbffb077777777700bfbbb077777777700000077777709999999999990700000000000000000aaaaaaaaaaaaaa00aaaaaaaaaaaaaa0
77700b00bbb007777700bbb00b0077777770ff0bffbb077770700bffbbb007777099999999999907000000cccc0000000a999999999999a00a111111111111a0
770f00ffffb0f07770f0bffff00f07777700bff0bb0bb077700ff0bbffbbb077709999999999990700000ccfccc000000a999999999999a00a011111111110a0
770f0f0000f0f07770f0f0000f0f077770000bff00f0bb07700bff0bb0bbb07770999999999999070000ccfffccc00000a999999999999a00a001111111100a0
770f00000000f07770f00000000f07777777000b0bf0bb077700bff00f0bb07770999999999999070000cccf0ccc00000a999999999999a00a001111111100a0
770bb000000bb07770bb000000bb07777700f0f00ff0b077777000b0bf0b077770999999999999070000cc0000cc00000a999999999999a00a000111111000a0
7770ff0ff0ff0777770ff0ff0ff07777770ff0ff0ff00777700f0f00ff0b077770999999999999070000cc0000cc00000a999999999999a00a000000000000a0
7770bf0ff0fb0777770bf0ff0fb077777770bfff0fb0777770ff0ff0ff00777770aaaaaaaaaaaa07000f0cc00cc0f0000aaaaaaaaaaaaaa00aaaaaaaaaaaaaa0
770b0bffffb0b07770b0bffffb0b077777770bbb0b007777770bfff0fb0777777000000000000007000f00cccc00f00000000000000000000000000000000000
70fb00000000b07770b00000000bf0777777000000b0777777700000000777777044444444444407000cff0000ffc00008a000a00a000a8008a000a00a000a80
70f00bbbb0ffb07770bff0bbbb00f07777770b0ff0bb077777770ff0bb007777704444444444440700000cffffc0000008a000a00a000a8008a000a00a000a80
7700bffff0ff0777770ff0ffffb0077777770f0ff0bb077777770ff0bb0f07777044444444444407000fc000000cf00008a000aaaa000a8008a000aaaa000a80
7770000bbb00777777700bbb0000777777770b00000007777770000bb00f07777044444444444407000f00000000f00008a0000000000a8008a0000000000a80
7700bff00000077777000000ffb007777777000ffff07777770ffff000ff07777044444444444407000000000000000008aaaaaaaaaaaa8008aaaaaaaaaaaa80
77700000000077777770000000007777777000000000077777000000000007777000000000000007000000000000000000000000000000000000000000000000
77777777777777777777777777777777777777000007777777777777777777777700000007777777000000000000000000000000000000000000000000000000
777700000777777777777000007777777777700bfbb07777777770000007777770ff0ffff07777770e88888888888888888888888888888888888888888888e0
77770ffff077777777770ffff0777777777000bffb00077770700bfffbb000777000f0000f00777708e888008888800888880088888008888800888880088e80
7700f0000f0077777700f0000f007777770f0bbffbb0f077700ff0bbfffbbb0770f00bffb00f0777088e88888ee88888ee88888ee88888ee88888ee88888e880
70f00bffb00f077770f00bffb00f0777770f000b0000f077700bff0bb0bbbb0770f0bbffbb0f07770888eeeee88eeeee88eeeee88eeeee88eeeee88eeeee8880
70f0bbffbb0f077770f0bbffbb0f0777770f0ffffff0f0777700bff00f0bb07770b0bbfbbb0b07770888e00000000000000000000000000000000000000e8080
70f0b0bfbb0f077770f0bbfb0b0f0777700bb000000bb077770000b0bf0b07777700bbfb0b0077770808e0e8888888888888888888888888888888888e0e8080
70b00bbfbb0b077770b0bbfbb00b07770ff0fb0000bf0777700f0f00ff0b077777700bbfb00777770808e08e888888888ee8888ee8888ee888888888e80e8880
77000bbbb000777777000bbbb000777770f0bf0ff0fb077770ff0ff0ff007777770bb0bbb0b077770888e088e888888880088880088880088888888e8808e880
770b0bbb0b0b077770b0b0bbb0b0777777000bffffb07777770bfff0fb077777770bfb0bb0bb0777088e80888e8888888888888888888888888888e88808e880
70fb0bb0bb0b077770b0bb0bb0bf07777770b000000b07777000000000077777770bbfb000bff077088e8088886888ee8888ee8888ee8888ee888688880e8880
70f0b00bbfb00777700bfbb00b0f077777000bfbb0bb07770ff0bbb0bfb007777700bbffbb0ff0770888e08888868800888800888800888800886888880e8080
770bfffffbb07777770bbfffffb07777770bb0bff00007770ff0bb0ffbb0f07777700bbb000007770808e08888886888888888888888888888868888880e8080
7700bbbbb000777777000bbbbb0077777770000bb0ff07777000000bbb00f07777770000ff0777770808e088888886eeeeeeeeeeeeeeeeeeee688888880e8880
770000000ff00777700ff000000077777777770000ff07777770fff0000ff07777777000ff0077770888e08888e08e66666666666666666666e80e88880e8880
777000000000777777000000000777777777777000007777770000000000007777777770000077770888e08888e08e6eeeeeeeeeeeeeeeeee6e80e888808e880
00000000000000008888888888888888000000007700007777777007700777770000000000000000088e808888888e6eeeeeeeeeeeeeeeeee6e888888808e880
00000bbffbb000008eeeeeeeeeeeeee804a9999070aaaa077777090770907777000cc00000880880088e808e08888e6eeeeeeeeeeeeeeeeee6e88880e80e8880
00ff0bbffbb0ff008e022222200000e804a999900a9999a0777098077089077700c00d00008eee800888e08e08888e6eeeeeeeeeeeeeeeeee6e88880e80e8880
00b00bbbbbb00b008e02222220eee0e804a999900a9999a0770980888808907700c0dd000008e8000888e08888888e6eeeeeeeeeeeeeeeeee6e88888880e8080
0000bb0000bb00008e00000000eee0e804a999900a9999a0709808888880890700c0dd00000080000808e08888e08e6eeeeeeeeeeeeeeeeee6e80e88880e8080
000000ffff0000008e00000000eee0e804aaaaa009aaaa90098088088088089000c00d00000000000808e08888e08e6eeeeeeeeeeeeeeeeee6e80e88880e8880
000b0ff00ff0b0008e00008880eee0e804444440709999070000880880880000000dd000000000000888e08888888e6eeeeeeeeeeeeeeeeee6e888888808e880
0f0b0bb00bb0b0f08e00008880eee0e8000000007700007777708888888807770000000000000000088e808e08888e6eeeeeeeeeeeeeeeeee6e88880e808e880
0b0b00bffb00b0b08e00008880eee0e8772000777797797777708888888807770000000000000000088e808e08888e6eeeeeeeeeeeeeeeeee6e88880e80e8880
000bbb0000bbb0008e00008880eee0e87202200777911977000008888880000000000400000000000888e08888888e6eeeeeeeeeeeeeeeeee6e88888880e8080
0f0bbbbbbbbbb0f08e02208880eee0e872022007791990970980008888000890000044000000cc000808e08888e08e6eeeeeeeeeeeeeeeeee6e80e88880e8080
0ff0bbbffbbb0ff08e02208880eee0e87720007779100097709800000000890700099400000c0c000808e08888e08e6eeeeeeeeeeeeeeeeee6e80e88880e8880
000b00bbbb00b0008e02208880eee0e87772077777900977770980000008907700094000000ccd000888e08888888e6eeeeeeeeeeeeeeeeee6e88888880e8880
000fb000000bf0008e000000000000e87772077777790777777098077089077700049000000000000888e08e08888e6eeeeeeeeeeeeeeeeee6e88880e808e880
00fbbb00000bbf008eeeeeeeeeeeeee87772007777790077777709077090777700000000000ddd00088e808e08888e6eeeeeeeeeeeeeeeeee6e88880e808e880
00000000000000008888888888888888777022777779997777777007700777770000000000000000088e808888888e6eeeeeeeeeeeeeeeeee6e88888880e8880
00000000000000000000000000000000000000000000000011ccccc111dccc11bbb00bbbbbb00bbb0888e08888e08e6eeeeeeeeeeeeeeeeee6e80e88880e8880
0000000000000000011111111111111000aaa9a009999900c1dccc11111dd11c33000033330000330888e08888e08e66666666666666666666e80e88880e8080
0066ddddddddd0000166ddd00dddd6100aa88800a0888990c11dd111cc1111dc30000003300000030808e088888886eeeeeeeeeeeeeeeeeeee688888880e8080
00dd6ddddddd000001dd6ddddddd61100a8880a0aa088890c1111111dcc111dc00000000000000000808e08888886888888888888888888888868888880e8880
00dddcccccc0000001ddd666666611100a880aaa8aa0889011111cc11dcc111d00000000000000000888e088888688008888008888008888008868888808e880
00dddcddddc0000001ddd600006111100a880aa888a088901111cccc1dccc1110000000000000000088e8088886888ee8888ee8888ee8888ee8886888808e880
00dddcdccdc0000001dd0600006011100a880aa088a08890c11cc1cc11dcc1cc0000000000000000088e80888e8888888888888888888888888888e8880e8880
00dddcdccdc0000001d006d00d6001100aa880a00a088990cc1cccccc1dcc1cc00000000000000000888e088e888888880088880088880088888888e880e8080
00dddcddddc000000160d6d00d610d100aaaa9a999999990c11cccccc1dc1ccc00000000000000000808e08e888888888ee8888ee8888ee888888888e80e8080
00dddcccccc00000016dd66666611d1004aa9a9a99999940cc1dcc1cd11d1ccc00000000000000000808e0e8888888888888888888888888888888888e0e8880
00dd00000006000001dd6111111611100a444444444444901d11dccd11111dcc00000000000000000888e00000000000000000000000000000000000000e8880
00d000000000600001d61110011161100aaaaa9a99999990cd111dd111ccc1dc00000000000000000888eeeee88eeeee88eeeee88eeeee88eeeee88eeeee8880
0000000000000600016111100111161004aaa0a9a9099940d11c11111cccc11d0000000000000000088e88888ee88888ee88888ee88888ee88888ee88888e880
00000000000006000161111dd1111610004aaa0a90999400111ccc11cc11cc11000000000000000008e888008888800888880088888008888800888880088e80
00000000000000000111111111111110090444444444409011ccccc1cccccc1133000033330000330e88888888888888888888888888888888888888888888e0
00000000000000000000000000000000000000000000000011cc1cc111cc1c11bbb00bbbbbb00bbb000000000000000000000000000000000000000000000000
0000000000000000000000080000000000080000000000000000000000000000e6e88888880888800888808888888e6eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
0000000000000000000000088000000000088000000000000000000000000000e6e88888880888088088808888888e6ee666666666666666666666666666666e
0000000000000000000000088000000088098000000000000000000000000000e6e88880e80880888808808e08888e6ee6eeeeeeeeeeeeeeeeeeeeeeeeeeee6e
000000000000000000000088800000000889aa98000000000000000000000000e6e88880e80808888880808e08888e6ee6e08888888888888888888888880e6e
00000000000000000000008988000000009a7980000000000000000000000000e6e88888880088888888008888888e6ee6e80888800888888888800888808e6e
00000000000000000000088a98000000009a9800000000000000000000000000e6e80e88880000000000008888e08e6ee6e880888ee8888888888ee888088e6e
0000000000000000000008899888880008900880000000000000000000000000e6e80e88808888888888880888e08e6ee6e88808888888888888888880888e6e
000000000000000000000889aa9a988808000000000000000000000000000000e6e888880888ee8888ee888088888e6ee6e88880888800888800888808888e6e
0000000c6c0000000000889aaaa9888000000000000000000000000000000000e6e88880888800888800888808888e6ee6e888880888ee8888ee888088888e6e
0000011111110000088899aa7a98880000000000000000000000000000000000e6e88808888888888888888880888e6ee6e80e88808888888888880888e08e6e
00001111111110008889a99aa999800000000000000000000000000000000000e6e880888ee8888888888ee888088e6ee6e80e88880000000000008888e08e6e
00011555555511000088889988a9800000000000000000000000000000000000e6e80888800888888888800888808e6ee6e88888880088888888008888888e6e
000151d11d115100000008988889800000000000000000000000000000000000e6e08888888888888888888888880e6ee6e88880e80808888880808e08888e6e
001151616d11c110000088880088800000000000000000000000000000000000e6eeeeeeeeeeeeeeeeeeeeeeeeeeee6ee6e88880e80880888808808e08888e6e
001c1161661d1c10000088000008800000000000000000000000000000000000e666666666666666666666666666666ee6e88888880888088088808888888e6e
000c117165161c00000080000000800000000000000000000000000000000000eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee6e88888880888800888808888888e6e
