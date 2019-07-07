
x = 0
y = 0
r = 0
function _draw()
	cls()

	if flr(r) % 2 == 0 then
		circfill(64, 64, r, 8)
	else
		circfill(64, 64, r, 9)
	end

	--line(96, r, 32, 96)
	
	--print(tostring(shl(-1, 15)))

    r = r + 0.1
    --x = (x + 1) % 128
    --y = (y + 1) % 128
end