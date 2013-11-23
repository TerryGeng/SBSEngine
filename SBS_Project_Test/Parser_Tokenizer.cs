
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using LexiconRules = System.Collections.Generic.Dictionary<SBSEngine.TokenizerType.LexiconType, SBSEngine.TokenizerType.LexiconRule>;

public static class TokenizerType
{
	public enum LexiconType
	{
		Undefined,
		LInteger,
		LFloat,
		LName,
		LString,
		LBlank,
		LCrLf,
		LSymbol
	}

	public struct Token
	{
		public LexiconType Type;
		public string Value;
	}

	public class LexiconRule
	{
		public delegate ScannerStatus ScannerFunction(char Character, int Position);
		public LexiconType Type;

		public ScannerFunction Scan;
		public LexiconRule(LexiconType Type, ScannerFunction Scanner)
		{
			this.Type = Type;
			this.Scan = Scanner;
		}
	}

	public enum ScannerStatus
	{
		/// <summary>
		/// Current and previous characters still match the rule. Match process continue.
		/// </summary>
		/// <remarks></remarks>
		Continued,
		/// <summary>
		/// The emergence of current char indicated this combination unmatched the rule.
		/// </summary>
		/// <remarks></remarks>
		Unmatch,
		/// <summary>
		/// Current and previous characters matchs the rule. Match process end.
		/// </summary>
		/// <remarks></remarks>
		Finished,
		/// <summary>
		/// Only previous characters match the rule. Match process end.
		/// </summary>
		/// <remarks></remarks>
		PreviousFinished

	}

	public class UnexpectedCharacterException : ApplicationException
	{

		public int Position;

		public char Character;
		public UnexpectedCharacterException(int Position, char Character) : base(string.Format("Unexpected Character '{0}'({1}) at {2}.", Character, Convert.ToInt32(Character), Position))
		{
			this.Position = Position;
			this.Character = Character;
		}
	}
}

static class TokenizerRules
{
	public static void LoadLexicalRules(LexiconRules Container)
	{
		Container.Add(LexiconType.LInteger, new LexiconRule(LexiconType.LInteger, IntegerScanner));
		Container.Add(LexiconType.LBlank, new LexiconRule(LexiconType.LBlank, BlankScanner));
		Container.Add(LexiconType.LName, new LexiconRule(LexiconType.LName, NameScanner));
		Container.Add(LexiconType.LCrLf, new LexiconRule(LexiconType.LCrLf, CrLfScanner));

	}

	public static ScannerStatus IntegerScanner(char Character, int Position)
	{
		if (char.IsDigit(Character))
			return ScannerStatus.Continued;
		return ScannerStatus.PreviousFinished;
	}

	public static ScannerStatus BlankScanner(char Character, int Position)
	{
		// Character is neither vbCr nor vbLf
		if ((!char.IsControl(Character)) && char.IsWhiteSpace(Character))
			return ScannerStatus.Continued;
		return ScannerStatus.PreviousFinished;
	}

	public static ScannerStatus NameScanner(char Character, int Position)
	{
		// Variable name cannot start with digit. Otherwise it will be read as two parts.
		if ((char.IsLetterOrDigit(Character)) || (Character == '_'))
			return ScannerStatus.Continued;

		return ScannerStatus.PreviousFinished;
	}

	public static ScannerStatus CrLfScanner(char Character, int Position)
	{
		switch (Position) {
			case 0:
				if (Character == Constants.vbCr)
					return ScannerStatus.Continued;
				break;
			case 1:
				if (Character == Constants.vbLf)
					return ScannerStatus.Finished;
				break;
		}
		return ScannerStatus.Unmatch;
	}
}

public class Tokenizer
{
	SourceCodeReader Reader;

	LexiconRules RulesContainer;
	public Tokenizer(string Code)
	{
		Reader = new SourceCodeReader(Code);
		RulesContainer = new LexiconRules();
		TokenizerRules.LoadLexicalRules(RulesContainer);
	}

	public Token NextToken()
	{
		if (Strings.AscW(Reader.Peek()) == 0)
			return null;
		dynamic Rules = RulesContainer.Keys;
		BitArray Matches = new BitArray(Rules.Count, true);

		StringBuilder TokenBuffer = new StringBuilder();
		char Character = '\0';
		int CandidatePos = 0;

		do {
			CandidatePos = 0;
			Character = Reader.NextChar();

			while (CandidatePos < Rules.Count) {
				if (!Matches(CandidatePos)){CandidatePos += 1;continue;}
				LexiconType Candidate = Rules(CandidatePos);
				switch (RulesContainer(Candidate).Scan(Character, TokenBuffer.Length)) {
					case ScannerStatus.Continued:
						break; // TODO: might not be correct. Was : Exit Select

						break;
					case ScannerStatus.Finished:
						TokenBuffer.Append(Character);
						return new Token {
							Type = Candidate,
							Value = TokenBuffer.ToString

						};
					case ScannerStatus.PreviousFinished:
						if (TokenBuffer.Length > 0) {
							Reader.MovePrev();
							return new Token {
								Type = Candidate,
								Value = TokenBuffer.ToString
							};
						} else {
							Matches.Set(CandidatePos, false);
						}
						break;
					case ScannerStatus.Unmatch:
						Matches.Set(CandidatePos, false);
						break;
				}
				CandidatePos += 1;
			}

			//Judge if there's still some candidates are true.
			foreach (bool IsMatch in Matches) {
				if (IsMatch) {
					TokenBuffer.Append(Character);
					continue;
				}
			}

			throw new UnexpectedCharacterException(Reader.Position, Character);
		} while (!(Strings.AscW(Character) == 0));

		return null;
	}

}

public class SourceCodeReader
{
	string BaseString;

	int Pointer;
	public int Position {
		get { return Pointer; }
	}

	public SourceCodeReader(ref string BaseString)
	{
		this.BaseString = BaseString;
	}

	public char Peek()
	{
		if (Pointer < 0 || Pointer >= BaseString.Length) {
			return Strings.ChrW(0);
		} else {
			return BaseString(Pointer);
		}
	}

	public char NextChar()
	{
		char result = Peek();
		MoveNext();
		return result;
	}

	//Reserve
	public bool NextChar(char ch)
	{
		if (Peek() == ch) {
			MoveNext();
			return true;
		} else {
			return false;
		}
	}

	public void MoveNext()
	{
		Pointer += 1;
	}

	public void MovePrev()
	{
		Pointer -= 1;
	}
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
