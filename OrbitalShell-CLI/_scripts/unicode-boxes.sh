#!orbsh
# draw boxes using unicodes characters (fgz - 19/1/11)
echo "(EdgeTopLeft,EdgeTopRight)1 (2 lines box)(br)(EdgeBottomLeft,EdgeBottomRight)2"
echo "(EdgeTopLeft,BarHorizontal,BarHorizontal,BarHorizontal,EdgeTopRight)1 (3 lines box)(br)(BarVertical) ? (BarVertical)2(br)(EdgeBottomLeft,BarHorizontal,BarHorizontal,BarHorizontal,EdgeBottomRight)3"

echo "(EdgeTopLeft,EdgeTopRight)(br)(EdgeBottomLeft,EdgeBottomRight)"
echo "(EdgeTopLeft,BarHorizontal,BarHorizontal,BarHorizontal,EdgeTopRight)(br)(BarVertical) ? (BarVertical)(br)(EdgeBottomLeft,BarHorizontal,BarHorizontal,BarHorizontal,EdgeBottomRight)"

echo "(EdgeDoubleTopLeft,EdgeDoubleTopRight)1 (2 lines box)(br)(EdgeDoubleBottomLeft,EdgeDoubleBottomRight)2"
echo "(EdgeDoubleTopLeft,BarDoubleHorizontal,BarDoubleHorizontal,BarDoubleHorizontal,EdgeDoubleTopRight)1 (3 lines box)(br)(BarDoubleVertical) ? (BarDoubleVertical)2(br)(EdgeDoubleBottomLeft,BarDoubleHorizontal,BarDoubleHorizontal,BarDoubleHorizontal,EdgeDoubleBottomRight)3"

echo "(EdgeDoubleTopLeft,EdgeDoubleTopRight)(br)(EdgeDoubleBottomLeft,EdgeDoubleBottomRight)"
echo "(EdgeDoubleTopLeft,BarDoubleHorizontal,BarDoubleHorizontal,BarDoubleHorizontal,EdgeDoubleTopRight)(br)(BarDoubleVertical) ? (BarDoubleVertical)(br)(EdgeDoubleBottomLeft,BarDoubleHorizontal,BarDoubleHorizontal,BarDoubleHorizontal,EdgeDoubleBottomRight)"
