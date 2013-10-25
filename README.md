# SBS Engine
This is an basic implementation of SBS interpreter.

**The core of this engine is experiencing a big modification(core write), which is expected to make this engine faster and support more features.**

## About this
This engine is written in Visual Basic.Net by a fool, with *a number of bugs and very low speed* .

The author is still trying his best to make this engine faster and have less bugs.

### Process of core rewrite
1. ■ Tokenizer - Source code to token
2. □ Parser - token to syntax tree
3. □ Converter - syntax tree to bytecode
4. □ Executor - run the bytecode

## About SBS
SBS - **Simple Basic Script** , a script language based on Basic language.

### Hello world in SBS
#### Print a hello-world
    Print("Hello world\n")

#### Print 100 hello-worlds
	For $i=0 To 100
    	Print("Hello world\n")
	End For
