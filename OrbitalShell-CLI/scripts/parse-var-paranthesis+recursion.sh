#!orbsh
# variable in expression with parenthesis and/or recursion

set x a
set ya "trouvé aussi!"
set a trouvé!
set xa "(f=red)!should not!"
set Aa "(f=green)!ok!"
set Aaa "(f=green)!ok! (f=yellow,b=darkgreen)2"
set e "(f=red)#failed#"

var local
echo

echo "(f=green)# x=a"
echo

echo "(f=green)# neutralization: \\*(e}"
echo "(f=green)# \${e}"
echo

echo "(f=green)# *{*{x}}"
echo ${${x}}
echo

echo "(f=green)# *{y*{x}}"
echo ${y${x}}
echo

echo "(f=green)# *x*x"
echo $x$x
echo

echo "(f=green)# *{x}*{x}"
echo ${x}${x}
echo

echo "(f=green)# *x*{x}"
echo $x${x}
echo

echo "(f=green)# *{A*x}"
echo ${A$x}
echo

echo "(f=green)# *{A*x*x}"
echo ${A$x$x}
echo
