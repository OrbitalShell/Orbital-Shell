#!orbsh
# ---------------------------------------------------------------------------------------------------------
# this is the default orbsh user profile init script
# ---------------------------------------------------------------------------------------------------------

# set default foreground (linux wsl ubuntu fix)
echo "(df=gray)" -n

# set the prompt

#prompt "(RSTXTA)(b8=19) (b8=20) (exec=System.Environment.CurrentDirectory) (b8=19,f8=46) (b8=18) (b8=17) (br)(f=yellow) > (rdc)"
#prompt "(RSTXTA)(f=cyan)(uon)(exec=System.Environment.CurrentDirectory)(tdoff)(br) (f=yellow,b=darkblue)>(rdc) "
prompt "(RSTXTA)(f=green)(uon)(exec=[[OrbitalShell.Lib.FileSystem.FileSystemPath.UnescapePathSeparators(System.Environment.CurrentDirectory)]])(tdoff)(br)(f=black,b=green)>(rdc) "
#prompt "(RSTXTA)(b8=19) (b8=20) $USERDOMAIN (b8=19,f8=46) (b8=18,f=yellow)>(b8=17) (rdc)"
#prompt "(RSTXTA)(f=cyan)$USERDOMAIN (f=white)> (rdc)"
#prompt "(RSTXTA) > "

# samples

#echo "(b8=196) (b8=202) (b8=208) (b8=214) (b8=220) (b8=226) (b8=190) (b8=154) (b8=118) (b8=82) (b8=46) (b8=41) (b8=36) (b8=31) (b8=26) (b8=21) (b8=20) (b8=19) (b8=54) (b8=90) (b8=126) (b8=162) (b8=198) (b8=204)"

# dev aliases

alias proj "cd '$shell/../../../..'"
alias vsproj "cd \"$home/documents/visual studio 2019/projects/applications\""

# git aliases

alias gss "git status -s -b -u -M --porcelain"
alias gs "git status"
alias ga "git add . && git status"

# modules aliases

alias mf "find $modules --top --fullname --dir"

# command aliases

alias dirs "dir -r -d"

# add usefull variables to shell environment

set scripts $shell/scripts

# advice(s)

echo "TIP: try module -f to list available modules, module -i {moduleId} to install a module in the shell, module -u {moduleId} to update an installed module, module --update-all [--check-only] to check/update all modules"
echo ""
