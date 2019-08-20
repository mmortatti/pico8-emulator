__lua__

a = 0
b = 0
d = 0
c = 0
if (a!=b)    
then
a=b
end

function foo(l)
	l(a)
	return a
end

a += 0b1010.10

a += b - (c - d) * (c == 8 and 0 or 10)