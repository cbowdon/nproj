all: test

build:
		xbuild /verbosity:minimal NProj.fsproj

test: clean build
		mkdir -p Test
		echo "namespace Test" > Test/Test.fs
		./bin/Debug/NProj.exe init Test
		./bin/Debug/NProj.exe add Test/Test.fs --project Test
    xbuild /verbository:minimal Test/Test.fsproj

clean:
		rm -rf Test
