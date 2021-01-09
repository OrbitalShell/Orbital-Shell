@echo add,commit and push with message: %1
@cd dotnet-console-app-toolkit
@git status
@git add -Av
@git commit -m %1
@git push -u origin master
@cd ..

@cd dotnet-console-app-toolkit-sample
@git status
@git add -Av
@git commit -m %1
@git push -u origin master
@cd ..

@cd dotnet-console-app-toolkit-shell
@git status
@git add -Av
@git commit -m %1
@git push -u origin master
@cd ..