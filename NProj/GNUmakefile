all: build test

build:
		xbuild NProj.fsproj

test:
		xbuild NProj.fsproj
		touch Test.fsproj
		./bin/Debug/NProj.exe init Test.fsproj
		./bin/Debug/NProj.exe add Test.fs --project Test.fsproj

clean:
		rm Test.fsproj
