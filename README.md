# SBS Engine
This is an basic implementation of SBS interpreter.

**The core of this engine is experiencing a big modification(we call it *core rewrite*), which is expected to make this engine faster and support more features.**

## About this
This engine is written in **C#**(which was Visual Basic.Net before) by a fool, with *a number of bugs and very low speed* .

The author is still trying his best to make this engine faster and have less bugs.

### Authors
In the beginning, this is merely my own project. However, after my first version released, [Henry(jhk)](https://github.com/jhk001) came to join me.

Now, I'm in charge of the main structure and coding, his work is to give me some advice, review my code and sometimes help me write some part.

### Process of core rewrite
1. ● Tokenizer - Source code to token
2. ◔ Parser - token to syntax tree
3. ○ Converter - syntax tree to bytecode
4. ○ Executor - run the bytecode

## About SBS
SBS - **Simple Basic Script** , a script language based on Basic language.

### Hello world in SBS
#### Print a hello-world
	Print("Hello world\n")

#### Print 100 hello-worlds
	For $i=0 To 100
    		Print("Hello world\n")
	End For
