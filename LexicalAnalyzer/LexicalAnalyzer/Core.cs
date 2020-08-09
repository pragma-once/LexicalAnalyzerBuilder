using System;
using System.Collections.Generic;

namespace LexicalAnalyzer.Core
{
    public abstract class Interface
    {
        public Interface()
        {
            sm = new StateMachine();
        }

        StateMachine sm;

        public abstract void Print(string Message);
    }

    public struct Return
    {
        public Return(int Offset, string TypeName)
        {
            this.Offset = Offset;
            this.TypeName = TypeName;
        }

        public int Offset;
        public string TypeName;
    }

    public class Transition
    {
        public bool InputFromSet = false;
        public string Input = null;

        public List<Return> Returns = new List<Return>();
        public string Hint = null;

        public string Error = null;
        public string Warning = null;

        public string DestinationState = null;

        public bool Clear
        {
            set
            {
                if (value)
                    Returns.Add(new Return(0, null));
            }
        }
		public string PreReturn
        {
            set
            {
                Returns.Add(new Return(-1, value));
            }
        }
		public string PostReturn
        {
            set
            {
                Returns.Add(new Return(0, value));
            }
        }
    }
	
    public struct Range
    {
        public Range(char Min, char Max)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public char Min;
        public char Max;
    }
    public class Set
    {
        public Set()
        {
            
        }

		public Set(params string[] Items)
		{
            AddItems(Items);
		}

		public Set(params Range[] Ranges)
		{
            AddRanges(Ranges);
		}

        public bool ContainsEmptyString = false;
        public List<string> Items = new List<string>();
        public List<Range> Ranges = new List<Range>();

        public void AddItems(params string[] Items)
        {
            foreach (string item in Items)
            {
                this.Items.Add(item);
            }
        }

		public void AddRanges(params Range[] Ranges)
		{
			foreach (Range range in Ranges)
			{
				this.Ranges.Add(range);
			}
		}
    }

	public class State
	{
		public List<Transition> Transitions = new List<Transition>();
        public Transition ElseTransition = null;
		public bool IsFinal = false;
	}

    public enum TokenMessageType : byte
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Hint = 255
    }

	public class Token
	{
        public int CharacterIndex = -1;
		public int LocationX = -1;
		public int LocationY = -1;

		public string Type = null;
		public string Value = null;

        public TokenMessageType MessageType = TokenMessageType.None;
        public bool IsMessage
        {
            get { return MessageType != TokenMessageType.None; }
        }

        public bool ShouldPrint
        {
            get { return IsMessage && (byte)MessageType < 128; }
        }

        public override string ToString()
        {
            if (IsMessage)
            {
                return MessageType.ToString() + " at (line: " + LocationY + ", col: " + LocationX + "): " + Value;
            }
            else
            {
                return "(line: " + LocationY + ", col: " + LocationX + "): " + Value;
            }
        }

        public static Token Error(int CharacterIndex, int LocationX, int LocationY, string Message)
        {
            return new Token()
            {
                MessageType = TokenMessageType.Error,
                CharacterIndex = CharacterIndex,
                LocationX = LocationX,
                LocationY = LocationY,
                Value = Message
            };
        }

		public static Token Warning(int CharacterIndex, int LocationX, int LocationY, string Message)
		{
			return new Token()
			{
				MessageType = TokenMessageType.Warning,
				CharacterIndex = CharacterIndex,
				LocationX = LocationX,
				LocationY = LocationY,
				Value = Message
			};
		}

		public static Token Hint(int CharacterIndex, int LocationX, int LocationY, string Message)
		{
			return new Token()
			{
				MessageType = TokenMessageType.Hint,
				CharacterIndex = CharacterIndex,
				LocationX = LocationX,
				LocationY = LocationY,
				Value = Message
			};
		}

		public static Token Return(int CharacterIndex, int LocationX, int LocationY, string Type, string Value)
		{
            return new Token()
            {
                CharacterIndex = CharacterIndex,
                LocationX = LocationX,
                LocationY = LocationY,
                Type = Type,
                Value = Value
			};
		}
	}

    enum StatementType : byte
    {
        None = 0x00,
        Set = 0x10,
        TransitionWithStringInput = 0x20,
        TransitionWithIdInput = 0x21,
        TransitionWithSetDefined = 0x22,
        TransitionWithNullInput = 0x23,
        Function = 0x30,
        Final = 0x40
    }

    class StatementInfo
    {
        public StatementInfo(StatementType Type)
        {
            this.Type = Type;
        }
        public StatementType Type = StatementType.None;
        public List<Token> Ids = new List<Token>();
        public List<Token> Parameters = new List<Token>();
    }

    /// <summary>
    /// A simple utility to help compile state machine program into state machine data much easier.
    /// Hard coded because it needs to be.
    /// </summary>
    static class CompileUtility
    {
		const string t_operator = "operator";
		//const string t_action = "action";
		const string t_id = "id";
		const string t_str1 = "str1";
		const string t_str2 = "str2";

        static StateMachine MakeTheLexicalAnalyzer()
        {
            StateMachine ProgramBuilder;

			ProgramBuilder = new StateMachine();
			ProgramBuilder.AddSet("ignore", new Set(";", "\n", "\t", " "));
			//ProgramBuilder.AddSet("action", new Set("return", "final", "else", "null"));
			Set ids = new Set();
			ids.AddRanges(new Range('a', 'z'), new Range('A', 'Z'), new Range('0', '9'));
			ids.AddItems("-", "_");
			ProgramBuilder.AddSet("idchar", ids);
			ProgramBuilder.AddSet("operator", new Set("...", ",", "=>", "=", "{", "}", "-", "(", ")", "$"));
			State S0 = new State();
			State id = new State();
			State str1 = new State();
			State str2 = new State();
			State str1_tmp = new State();
			State str2_tmp = new State();
			State comment = new State();
			S0.IsFinal = true;
            id.IsFinal = true;
			S0.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "#",
				DestinationState = "comment",
				Clear = true
			});
			S0.Transitions.Add(new Transition()
			{
				InputFromSet = true,
				Input = "ignore",
				DestinationState = "S0",
				Clear = true
			});
			S0.Transitions.Add(new Transition()
			{
				InputFromSet = true,
				Input = "operator",
				DestinationState = "S0",
				PostReturn = t_operator
			});
			//S0.Transitions.Add(new Transition()
			//{
			//  InputFromSet = true,
			//  Input = "action",
			//  DestinationState = "S0",
			//  PostReturn = t_action
			//});
			S0.Transitions.Add(new Transition()
			{
				InputFromSet = true,
				Input = "idchar",
				DestinationState = "id"
			});
			id.Transitions.Add(new Transition()
			{
				InputFromSet = true,
				Input = "idchar",
				DestinationState = "id"
			});
			id.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "#",
				DestinationState = "comment",
				PreReturn = t_id,
				Clear = true
			});
			id.Transitions.Add(new Transition()
			{
				InputFromSet = true,
				Input = "ignore",
				DestinationState = "S0",
				PreReturn = t_id,
				Clear = true
			});
			id.Transitions.Add(new Transition()
			{
				InputFromSet = true,
				Input = "operator",
				DestinationState = "S0",
				PreReturn = t_id,
				PostReturn = t_operator
			});
            //id.Transitions.Add(new Transition()
            //{
            //  InputFromSet = true,
            //  Input = "action",
            //  DestinationState = "S0",
            //  PreReturn = t_id,
            //  PostReturn = t_action
            //});
            id.Transitions.Add(new Transition()
            {
                InputFromSet = true,
                Input = null, // End of the string
                PreReturn = t_id
            });
            S0.ElseTransition = new Transition() { DestinationState = "S0", Warning = "Unknown character", Clear = true };
			id.ElseTransition = new Transition() { DestinationState = "S0", PreReturn = "id", Warning = "Unknown character", Clear = true };
			S0.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "'",
				DestinationState = "str1",
				Clear = true
			});
			S0.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "\"",
				DestinationState = "str2",
				Clear = true
			});
			id.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "'",
				DestinationState = "str1",
				PreReturn = t_id,
				Clear = true
			});
			id.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "\"",
				DestinationState = "str2",
				PreReturn = t_id,
				Clear = true
			});
			str1.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "'",
				DestinationState = "S0",
				PreReturn = t_str1,
				Clear = true
			});
			str2.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "\"",
				DestinationState = "S0",
				PreReturn = t_str2,
				Clear = true
			});
			str1.ElseTransition = new Transition() { DestinationState = "str1" };
			str2.ElseTransition = new Transition() { DestinationState = "str2" };
			str1.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "\\",
				DestinationState = "str1_tmp"
			});
			str2.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "\\",
				DestinationState = "str2_tmp"
			});
			str1_tmp.ElseTransition = new Transition() { DestinationState = "str1" };
			str2_tmp.ElseTransition = new Transition() { DestinationState = "str2" };
			comment.Transitions.Add(new Transition()
			{
				InputFromSet = false,
				Input = "#",
				DestinationState = "S0",
				Clear = true
			});
			comment.ElseTransition = new Transition() { DestinationState = "comment", Clear = true };
			ProgramBuilder.AddState("S0", S0);
            ProgramBuilder.AddState("$start", S0);
            ProgramBuilder.AddState("id", id);
			ProgramBuilder.AddState("str1", str1);
			ProgramBuilder.AddState("str2", str2);
			ProgramBuilder.AddState("str1_tmp", str1_tmp);
			ProgramBuilder.AddState("str2_tmp", str2_tmp);
			ProgramBuilder.AddState("comment", comment);

            return ProgramBuilder;
        }

        static StateMachine LexicalAnalyzer = null;

        public static List<Token> ProcessLexical(string Program, Interface Interface)
        {
            if (LexicalAnalyzer == null)
                LexicalAnalyzer = MakeTheLexicalAnalyzer();
            LexicalAnalyzer.Interface = Interface;
            if (LexicalAnalyzer.Process(Program, out List<Token> Tokens))
                return Tokens;
            else
                return null;
        }

        public static void ProcessSyntax(List<Token> Program, out List<StatementInfo> Statements, out Queue<string> Errors)
        {
            Statements = new List<StatementInfo>();
            Errors = new Queue<string>();

            List<Token> p = Program;
            List<StatementInfo> r = Statements;
            Queue<string> e = Errors;

            for (int i = 0; i < p.Count; i++)
            {
                StatementInfo stmt;
                try
                {
                    bool transition(Token id1)
                    {
                        i++;
                        if (p[i].Type == t_str1 || p[i].Type == t_str2)
                        {
                            stmt = new StatementInfo(StatementType.TransitionWithStringInput);
                            stmt.Ids.Add(id1);
                            stmt.Parameters.Add(p[i]);

                            i++;
                            if (p[i].Type == t_operator && p[i].Value == "=>")
                            {
                                i++;
                                if (p[i].Type == t_id)
                                {
                                    stmt.Ids.Add(p[i]);
                                    r.Add(stmt);
                                    return true;
                                }
                                else if (p[i].Type == t_operator && p[i].Value == "-")
                                {
                                    Token virtual_token = new Token()
                                    {
                                        CharacterIndex = p[i].CharacterIndex,
                                        LocationX = p[i].LocationX,
                                        LocationY = p[i].LocationY,
                                        Type = t_id,
                                        Value = null
                                    };
                                    stmt.Ids.Add(virtual_token);
                                    r.Add(stmt);
                                    i--;
                                    return true;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected a state name or a function.");
                                return false;
                            }
                            e.Enqueue(p[i].ToString());
                            e.Enqueue("Expected a '=>'.");
                            return false;
                        }
                        else if (p[i].Type == t_id)
                        {
                            stmt = new StatementInfo(StatementType.TransitionWithIdInput);
                            stmt.Ids.Add(id1);
                            stmt.Parameters.Add(p[i]);

                            i++;
                            if (p[i].Type == t_operator && p[i].Value == "=>")
                            {
                                i++;
                                if (p[i].Type == t_id)
                                {
                                    stmt.Ids.Add(p[i]);
                                    r.Add(stmt);
                                    return true;
                                }
                                else if (p[i].Type == t_operator && p[i].Value == "-")
                                {
                                    Token virtual_token = new Token()
                                    {
                                        CharacterIndex = p[i].CharacterIndex,
                                        LocationX = p[i].LocationX,
                                        LocationY = p[i].LocationY,
                                        Type = t_id,
                                        Value = null
                                    };
                                    stmt.Ids.Add(virtual_token);
                                    r.Add(stmt);
                                    i--;
                                    return true;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected a state name or a function.");
                                return false;
                            }
                            e.Enqueue(p[i].ToString());
                            e.Enqueue("Expected a '=>'.");
                            return false;
                        }
                        else if (p[i].Type == t_operator && p[i].Value == "{")
                        {
                            stmt = new StatementInfo(StatementType.TransitionWithSetDefined);
                            stmt.Ids.Add(id1);
                            do
                            {
                                i++;
                                if (p[i].Type == t_id || p[i].Type == t_str1 || p[i].Type == t_str2 || (p[i].Type == t_operator && p[i].Value == "..."))
                                {
                                    stmt.Parameters.Add(p[i]);
                                    i++;
                                    continue;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected a set name, a string, or a range.");
                                stmt = null;
                                while (!(p[i].Type == t_operator && p[i].Value == "}"))
                                    i++;
                                break;

                            } while (p[i].Type == t_operator && p[i].Value == ",");
                            if (p[i].Type == t_operator && p[i].Value == "}")
                            {
                                if (stmt != null)
                                {
                                    i++;
                                    if (p[i].Type == t_operator && p[i].Value == "=>")
                                    {
                                        i++;
                                        if (p[i].Type == t_id)
                                        {
                                            stmt.Ids.Add(p[i]);
                                            r.Add(stmt);
                                            return true;
                                        }
                                        else if (p[i].Type == t_operator && p[i].Value == "-")
                                        {
                                            Token virtual_token = new Token()
                                            {
                                                CharacterIndex = p[i].CharacterIndex,
                                                LocationX = p[i].LocationX,
                                                LocationY = p[i].LocationY,
                                                Type = t_id,
                                                Value = null
                                            };
                                            stmt.Ids.Add(virtual_token);
                                            r.Add(stmt);
                                            i--;
                                            return true;
                                        }
                                        e.Enqueue(p[i].ToString());
                                        e.Enqueue("Expected a state name or a function.");
                                        return false;
                                    }
                                    e.Enqueue(p[i].ToString());
                                    e.Enqueue("Expected a '=>'.");
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            e.Enqueue(p[i].ToString());
                            e.Enqueue("Expected a '}'.");
                            while (!(p[i].Type == t_operator && p[i].Value == "}"))
                                i++;
                            return true;
                        }
                        else if (p[i].Type == t_operator && p[i].Value == "$")
                        {
                            stmt = new StatementInfo(StatementType.TransitionWithNullInput);
                            stmt.Ids.Add(id1);
                            stmt.Parameters.Add(p[i]);

                            i++;
                            if (p[i].Type == t_operator && p[i].Value == "=>")
                            {
                                i++;
                                if (p[i].Type == t_id)
                                {
                                    stmt.Ids.Add(p[i]);
                                    r.Add(stmt);
                                    return true;
                                }
                                else if (p[i].Type == t_operator && p[i].Value == "-")
                                {
                                    Token virtual_token = new Token()
                                    {
                                        CharacterIndex = p[i].CharacterIndex,
                                        LocationX = p[i].LocationX,
                                        LocationY = p[i].LocationY,
                                        Type = t_id,
                                        Value = null
                                    };
                                    stmt.Ids.Add(virtual_token);
                                    r.Add(stmt);
                                    i--;
                                    return true;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected a state name or a function.");
                                return false;
                            }
                            e.Enqueue(p[i].ToString());
                            e.Enqueue("Expected a '=>'.");
                            return false;
                        }
                        e.Enqueue(p[i].ToString());
                        e.Enqueue("Expected a string, set name, set definition, or a '$'.");
                        return false;
                    }

                    if (p[i].Type == t_operator && p[i].Value == "(")
                    {
                        List<Token> id1s = new List<Token>();
                        i++;
                        bool Cont = true;
                        if (p[i].Type != t_id)
                        {
                            e.Enqueue(p[i].ToString());
                            e.Enqueue("Expected a state name.");
                            break;
                        }
                        while (true)
                        {
                            id1s.Add(p[i]);
                            i++;
                            if (p[i].Type == t_operator && p[i].Value == ",")
                            {
                                i++;
                                if (p[i].Type == t_id)
                                {
                                    continue;
                                }
                                else
                                {
                                    e.Enqueue(p[i].ToString());
                                    e.Enqueue("Expected another state name.");
                                    Cont = false;
                                    break;
                                }
                            }
                            else if (p[i].Type == t_operator && p[i].Value == ")")
                            {
                                i++;
                                break;
                            }
                            else
                            {
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected a ',' or ')'.");
                                Cont = false;
                                break;
                            }
                        }
                        if (Cont)
                        {
                            int saved_i = i;
                            foreach (Token id1 in id1s)
                            {
                                i = saved_i;
                                if (p[i].Type == t_operator && p[i].Value == ",")
                                {
                                    bool result = transition(id1);
                                    if (!result)
                                    {
                                        Cont = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    e.Enqueue(p[i].ToString());
                                    e.Enqueue("Expected ','.");
                                    Cont = false;
                                    break;
                                }
                            }
                            if (Cont)
                                continue;
                            else
                                break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (p[i].Type == t_id)
                    {
                        Token id1 = p[i];
                        i++;
                        if (p[i].Type == t_operator)
                        {
                            if (p[i].Value == ",")
                            {
                                bool result = transition(id1);
                                if (result)
                                    continue;
                                else
                                    break;
                            }
                            else if (p[i].Value == "=")
                            {
                                i++;
                                if (p[i].Type == t_operator && p[i].Value == "{")
                                {
                                    stmt = new StatementInfo(StatementType.Set);
                                    stmt.Ids.Add(id1);
                                    do
                                    {
                                        i++;
                                        if (p[i].Type == t_id || p[i].Type == t_str1 || p[i].Type == t_str2 || (p[i].Type == t_operator && p[i].Value == "..."))
                                        {
                                            stmt.Parameters.Add(p[i]);
                                            i++;
                                            continue;
                                        }
                                        e.Enqueue(p[i].ToString());
                                        e.Enqueue("Expected an id, a string, or a range.");
                                        stmt = null;
                                        while (!(p[i].Type == t_operator && p[i].Value == "}"))
                                            i++;
                                        break;

                                    } while (p[i].Type == t_operator && p[i].Value == ",");
                                    if (p[i].Type == t_operator && p[i].Value == "}")
                                    {
                                        if (stmt != null)
                                            r.Add(stmt);
                                        continue;
                                    }
                                    e.Enqueue(p[i].ToString());
                                    e.Enqueue("Expected a '}'.");
                                    while (!(p[i].Type == t_operator && p[i].Value == "}"))
                                        i++;
                                    continue;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected a '{'.");
                                break;
                            }
                            else if (p[i].Value == "-")
                            {
                                i++;
                                while (p[i].Type == t_operator && p[i].Value == "-")
                                {
                                    i++;
                                }
                                if (p[i].Type == t_id && p[i].Value == "final")
                                {
                                    stmt = new StatementInfo(StatementType.Final);
                                    stmt.Parameters.Add(id1);
                                    r.Add(stmt);
                                    continue;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Function not defined.");
                                continue;
                            }
                            else if (id1.Value == "final")
                            {
                                if (p[i].Value == "(")
                                {
                                    stmt = new StatementInfo(StatementType.Final);
                                    do
                                    {
                                        i++;
                                        if (p[i].Type == t_id)
                                        {
                                            stmt.Parameters.Add(p[i]);
                                            i++;
                                            continue;
                                        }
                                        e.Enqueue(p[i].ToString());
                                        e.Enqueue("Expected an id.");
                                        stmt = null;
                                        while (!(p[i].Type == t_operator && p[i].Value == ")"))
                                            i++;
                                        break;

                                    } while ((p[i].Type == t_operator && p[i].Value == ","));
                                    if (p[i].Type == t_operator && p[i].Value == ")")
                                    {
                                        if (stmt != null)
                                            r.Add(stmt);
                                        continue;
                                    }
                                    e.Enqueue(p[i].ToString());
                                    e.Enqueue("Expected a ',' or a ')'");
                                    while (!(p[i].Type == t_operator && p[i].Value == ")"))
                                        i++;
                                    continue;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected '('.");
                                continue;
                            }
                            e.Enqueue(p[i].ToString());
                            e.Enqueue("Expected ',', '=', or '-final'.");
                            break;
                        }
                        e.Enqueue(p[i].ToString());
                        e.Enqueue("Expected an operator.");
                        break;
                    }
                    else if (p[i].Type == t_operator && p[i].Value == "-")
                    {
                        i++;
                        while (p[i].Type == t_operator && p[i].Value == "-")
                        {
                            i++;
                        }
                        if (p[i].Type == t_id)
                        {
                            stmt = new StatementInfo(StatementType.Function);
                            stmt.Ids.Add(p[i]);
                            i++;
                            if (i >= p.Count)
                            {
                                i--;
                                r.Add(stmt);
                                continue;
                            }
                            if (p[i].Type == t_operator && p[i].Value == "(")
                            {
                                do
                                {
                                    i++;
                                    if (p[i].Type == t_operator && p[i].Value == "-")
                                    {
                                        i++;
                                        if (p[i].Type == t_id)
                                        {
                                            stmt.Parameters.Add(p[i]);
                                            i++;
                                            continue;
                                        }
                                        e.Enqueue(p[i].ToString());
                                        e.Enqueue("Expected a number.");
                                        while (!(p[i].Type == t_operator && p[i].Value == ")"))
                                            i++;
                                        continue;
                                    }
                                    if (p[i].Type == t_id || p[i].Type == t_str1 || p[i].Type == t_str2)
                                    {
                                        stmt.Parameters.Add(p[i]);
                                        i++;
                                        continue;
                                    }
                                    e.Enqueue(p[i].ToString());
                                    e.Enqueue("Expected a parameter.");
                                    stmt = null;
                                    while (!(p[i].Type == t_operator && p[i].Value == ")"))
                                        i++;
                                    break;

                                } while ((p[i].Type == t_operator && p[i].Value == ","));
                                if (p[i].Type == t_operator && p[i].Value == ")")
                                {
                                    if (stmt != null)
                                        r.Add(stmt);
                                    continue;
                                }
                                e.Enqueue(p[i].ToString());
                                e.Enqueue("Expected a ',' or a ')'");
                                while (!(p[i].Type == t_operator && p[i].Value == ")"))
                                    i++;
                                continue;
                            }
                            else
                            {
                                i--;
                                r.Add(stmt);
                                continue;
                            }
                        }
                        e.Enqueue(p[i].ToString());
                        e.Enqueue("Expected a function name.");
                        break;
                    }
                    e.Enqueue(p[i].ToString());
                    e.Enqueue("Unknown syntax.");
                    break;
                }
                catch (ArgumentOutOfRangeException)
                {
					e.Enqueue("Incomplete program.");
                    break;
                }
            }
        }

		static string ProcessStr(string str)
		{
			str = str.Replace("\\n", "\n");
			str = str.Replace("\\t", "\t");
            str = str.Replace("\\\\", "\\");
            str = str.Replace("\\'", "'");
            str = str.Replace("\\\"", "\"");
            // str = str.Replace("\\", "");    // (else/default) // this won't let '//' to be '/' and deletes them all

			return str;
		}

        public static void ProcessSemantic(List<StatementInfo> Statements,
                                           out Dictionary<string, Set> Sets,
                                           out Dictionary<string, State> States,
                                           out Queue<string> Errors)
        {
            Sets = new Dictionary<string, Set>();
            States = new Dictionary<string, State>();
            Errors = new Queue<string>();

            Transition last_transition = null;
            bool NoStartState = true;
            foreach (StatementInfo stmt in Statements)
            {
                foreach (Token token in stmt.Ids)
                {
                    if (token.Type == t_str1 || token.Type == t_str2)
                    {
                        token.Value = ProcessStr(token.Value);
                    }
                }
				foreach (Token token in stmt.Parameters)
				{
					if (token.Type == t_str1 || token.Type == t_str2)
					{
						token.Value = ProcessStr(token.Value);
					}
				}
                Set s;
                State s1;
                State s2;
                switch (stmt.Type)
                {
                    case StatementType.None:
                        break;
					case StatementType.Set:
                        s = new Set();
                        for (int i = 0; i < stmt.Parameters.Count; i++)
                        {
                            if (stmt.Parameters[i].Type == t_id)
                            {
                                if (Sets.TryGetValue(stmt.Parameters[i].Value, out Set ref_set))
                                {
                                    foreach (string item in ref_set.Items)
                                    {
                                        //if (stmt.Parameters[i].Value == "") // not necessary here ....
                                        //{
                                        //    Errors.Enqueue(stmt.Parameters[i].ToString());
                                        //    Errors.Enqueue("Empty string inputs are not allowed, they cause recursion.");
                                        //    break;
                                        //}
                                        s.Items.Add(item);
                                    }
                                    foreach (Range range in ref_set.Ranges)
                                    {
                                        s.Ranges.Add(range);
                                    }
                                }
                                else
                                {
                                    Errors.Enqueue(stmt.Parameters[i].ToString());
                                    Errors.Enqueue("The set " + stmt.Parameters[i].Value + " is not defined.");
                                    break;
                                }
                            }
                            else if (stmt.Parameters[i].Type == t_str1 || stmt.Parameters[i].Type == t_str2)
                            {
                                if (stmt.Parameters[i].Value == "")
                                {
                                    Errors.Enqueue(stmt.Parameters[i].ToString());
                                    Errors.Enqueue("Empty string inputs are not allowed, they cause recursion.");
                                    break;
                                }
                                s.Items.Add(stmt.Parameters[i].Value);
                            }
                            else if (stmt.Parameters[i].Type == t_operator && stmt.Parameters[i].Value == "...")
                            {
                                if ((stmt.Parameters[i - 1].Type == t_str1 || stmt.Parameters[i - 1].Type == t_str2)
                                    && (stmt.Parameters[i + 1].Type == t_str1 || stmt.Parameters[i + 1].Type == t_str2)
                                    && stmt.Parameters[i - 1].Value.Length == 1 && stmt.Parameters[i + 1].Value.Length == 1)
                                {
                                    if (s.Items[s.Items.Count - 1] == stmt.Parameters[i - 1].Value)
                                    {
                                        s.Items.RemoveAt(s.Items.Count - 1);
                                    }
                                    s.Ranges.Add(new Range(stmt.Parameters[i - 1].Value[0], stmt.Parameters[i + 1].Value[0]));
                                    i++;
                                }
                                else
                                {
                                    Errors.Enqueue(stmt.Parameters[i].ToString());
                                    Errors.Enqueue("Not a valid range. Ranges can only contain 2 single character strings.");
                                    break;
                                }
                            }
                            else
                            {
                                Errors.Enqueue(stmt.Parameters[i].ToString());
                                Errors.Enqueue("Unknown type.");
                                break;
                            }
                        }
                        Sets.Add(stmt.Ids[0].Value, s);
                        break;
					case StatementType.TransitionWithStringInput:
                        if (stmt.Parameters[0].Value == "")
                        {
                            Errors.Enqueue(stmt.Parameters[0].ToString());
                            Errors.Enqueue("Empty string inputs are not allowed, they cause recursion.");
                            break;
                        }

                        if (!States.TryGetValue(stmt.Ids[0].Value, out s1))
                        {
                            s1 = new State();
                            States.Add(stmt.Ids[0].Value, s1);
                        }

                        if (NoStartState)
                        {
                            States.Add("$start", s1);
                            NoStartState = false;
                        }

                        if (stmt.Ids[1].Value != null)
                            if (!States.TryGetValue(stmt.Ids[1].Value, out s2))
                            {
                                s2 = new State();
                                States.Add(stmt.Ids[1].Value, s2);
                            }
                        last_transition = new Transition();
                        last_transition.InputFromSet = false;
                        last_transition.Input = stmt.Parameters[0].Value;
                        last_transition.DestinationState = stmt.Ids[1].Value;

                        s1.Transitions.Add(last_transition);
						break;
					case StatementType.TransitionWithIdInput:
						if (!States.TryGetValue(stmt.Ids[0].Value, out s1))
						{
							s1 = new State();
                            States.Add(stmt.Ids[0].Value, s1);
                        }

                        if (NoStartState)
                        {
                            States.Add("$start", s1);
                            NoStartState = false;
                        }

                        if (stmt.Ids[1].Value != null)
							if (!States.TryGetValue(stmt.Ids[1].Value, out s2))
							{
								s2 = new State();
								States.Add(stmt.Ids[1].Value, s2);
							}
						last_transition = new Transition();
						last_transition.DestinationState = stmt.Ids[1].Value;

                        if (stmt.Parameters[0].Value == "else")
                        {
                            s1.ElseTransition = last_transition;
                        }
                        else if (Sets.TryGetValue(stmt.Parameters[0].Value, out Set tmp_useless_tmp))
                        {
                            last_transition.InputFromSet = true;
                            last_transition.Input = stmt.Parameters[0].Value;

                            s1.Transitions.Add(last_transition);
                        }
						else
						{
                            Errors.Enqueue(stmt.Parameters[0].ToString());
                            Errors.Enqueue("The set '" + stmt.Parameters[0].Value + "' is not defined.");
                            break;
                        }
						break;
					case StatementType.TransitionWithSetDefined:
						if (!States.TryGetValue(stmt.Ids[0].Value, out s1))
						{
							s1 = new State();
                            States.Add(stmt.Ids[0].Value, s1);
                        }

                        if (NoStartState)
                        {
                            States.Add("$start", s1);
                            NoStartState = false;
                        }

                        if (stmt.Ids[1].Value != null)
							if (!States.TryGetValue(stmt.Ids[1].Value, out s2))
							{
								s2 = new State();
								States.Add(stmt.Ids[1].Value, s2);
							}

						s = new Set();
						for (int i = 0; i < stmt.Parameters.Count; i++)
						{
							if (stmt.Parameters[i].Type == t_id)
							{
								if (Sets.TryGetValue(stmt.Parameters[i].Value, out Set ref_set))
								{
                                    foreach (string item in ref_set.Items)
									{
                                        //if (stmt.Parameters[i].Value == "") // NOT necessary here ! I hate bugs.
                                        //{
                                        //    Errors.Enqueue(stmt.Parameters[i].ToString());
                                        //    Errors.Enqueue("Empty string inputs are not allowed, they cause recursion.");
                                        //    break;
                                        //}
                                        s.Items.Add(item);
									}
									foreach (Range range in ref_set.Ranges)
									{
										s.Ranges.Add(range);
									}
								}
								else
								{
									Errors.Enqueue(stmt.Parameters[i].ToString());
									Errors.Enqueue("The set " + stmt.Parameters[i].Value + " is not defined.");
									break;
								}
							}
							else if (stmt.Parameters[i].Type == t_str1 || stmt.Parameters[i].Type == t_str2)
							{
                                if (stmt.Parameters[i].Value == "")
                                {
                                    Errors.Enqueue(stmt.Parameters[i].ToString());
                                    Errors.Enqueue("Empty string inputs are not allowed, they cause recursion.");
                                    break;
                                }
                                s.Items.Add(stmt.Parameters[i].Value);
							}
							else if (stmt.Parameters[i].Type == t_operator && stmt.Parameters[i].Value == "...")
							{
								if ((stmt.Parameters[i - 1].Type == t_str1 || stmt.Parameters[i - 1].Type == t_str2)
									&& (stmt.Parameters[i + 1].Type == t_str1 || stmt.Parameters[i + 1].Type == t_str2)
									&& stmt.Parameters[i - 1].Value.Length == 1 && stmt.Parameters[i + 1].Value.Length == 1)
								{
									if (s.Items[s.Items.Count - 1] == stmt.Parameters[i - 1].Value)
									{
										s.Items.RemoveAt(s.Items.Count - 1);
									}
									s.Ranges.Add(new Range(stmt.Parameters[i - 1].Value[0], stmt.Parameters[i + 1].Value[0]));
									i++;
								}
							}
							else
							{
								Errors.Enqueue(stmt.Parameters[i].ToString());
								Errors.Enqueue("Unknown type.");
								break;
							}
						}
                        string set_name = "$set-" + stmt.Ids[0].CharacterIndex;
						Sets.Add(set_name, s);

						last_transition = new Transition();
						last_transition.InputFromSet = true;
						last_transition.Input = set_name;
						last_transition.DestinationState = stmt.Ids[1].Value;

                        s1.Transitions.Add(last_transition);
						break;
                    case StatementType.TransitionWithNullInput:
                        if (!States.TryGetValue(stmt.Ids[0].Value, out s1))
                        {
                            s1 = new State();
                            States.Add(stmt.Ids[0].Value, s1);
                        }

                        if (NoStartState)
                        {
                            States.Add("$start", s1);
                            NoStartState = false;
                        }

                        if (stmt.Ids[1].Value != null)
                            if (!States.TryGetValue(stmt.Ids[1].Value, out s2))
                            {
                                s2 = new State();
                                States.Add(stmt.Ids[1].Value, s2);
                            }
                        last_transition = new Transition();
                        last_transition.InputFromSet = false;
                        last_transition.Input = null;
                        last_transition.DestinationState = stmt.Ids[1].Value;

                        s1.Transitions.Add(last_transition);
                        break;
                    case StatementType.Function:
                        if (last_transition == null)
                        {
                            Errors.Enqueue(stmt.Ids[0].ToString());
                            Errors.Enqueue("No transition defined. Please define the transition first.");
                        }
                        if (stmt.Ids[0].Value == "return")
                        {
                            if (stmt.Parameters.Count == 0)
                            {
                                last_transition.Returns.Add(new Return(0, null));
                            }
                            else if (stmt.Parameters.Count == 1)
                            {
                                if (stmt.Parameters[0].Type == t_id)
                                {
                                    if (stmt.Parameters[0].Value == "last")
                                    {
                                        last_transition.Returns.Add(new Return(-1, null));
                                    }
                                    else if (int.TryParse(stmt.Parameters[0].Value, out int offset))
                                    {
                                        last_transition.Returns.Add(new Return(offset, null));
                                    }
                                    else
                                    {
                                        Errors.Enqueue(stmt.Parameters[0].ToString());
                                        Errors.Enqueue("The offset should be an integer, or 'last'.");
                                    }
                                }
                                else if (stmt.Parameters[0].Type == t_str1 || stmt.Parameters[0].Type == t_str2)
                                {
                                    last_transition.Returns.Add(new Return(0, stmt.Parameters[0].Value));
                                }
                                else
                                {
                                    Errors.Enqueue(stmt.Ids[0].ToString());
                                    Errors.Enqueue("Wrong parameter(s).");
                                }
                            }
                            else if (stmt.Parameters.Count == 2 && stmt.Parameters[0].Type == t_id
                                     && (stmt.Parameters[1].Type == t_str1 || stmt.Parameters[1].Type == t_str2))
                            {
                                if (stmt.Parameters[0].Value == "last")
                                {
                                    last_transition.Returns.Add(new Return(-1, stmt.Parameters[1].Value));
                                }
                                else if (int.TryParse(stmt.Parameters[0].Value, out int offset))
                                {
                                    last_transition.Returns.Add(new Return(offset, stmt.Parameters[1].Value));
                                }
                                else
                                {
                                    Errors.Enqueue(stmt.Parameters[0].ToString());
                                    Errors.Enqueue("The offset should be an integer, or 'last'.");
                                }
                            }
                            else
                            {
                                Errors.Enqueue(stmt.Ids[0].ToString());
                                Errors.Enqueue("Wrong parameter(s).");
                            }
                        }
                        else if (stmt.Ids[0].Value == "clear" || stmt.Ids[0].Value == "ignore")
                        {
                            if (stmt.Parameters.Count == 0)
                            {
                                last_transition.Returns.Add(new Return(0, null));
                            }
                            else if (stmt.Parameters.Count == 1 && stmt.Parameters[0].Type == t_id)
                            {
                                if (stmt.Parameters[0].Value == "last")
                                {
                                    last_transition.Returns.Add(new Return(-1, null));
                                }
                                else if (int.TryParse(stmt.Parameters[0].Value, out int offset))
                                {
                                    last_transition.Returns.Add(new Return(offset, null));
                                }
                                else
                                {
                                    Errors.Enqueue(stmt.Parameters[0].ToString());
                                    Errors.Enqueue("The offset should be an integer, or 'last'.");
                                }
                            }
                            else
                            {
                                Errors.Enqueue(stmt.Ids[0].ToString());
                                Errors.Enqueue("Wrong parameter(s).");
                            }
                        }
                        else if (stmt.Ids[0].Value == "error")
                        {
                            if (stmt.Parameters.Count == 1)
                            {
                                if (stmt.Parameters[0].Type == t_str1 || stmt.Parameters[0].Type == t_str2)
                                {
                                    last_transition.Error = stmt.Parameters[0].Value;
                                    break;
                                }
                            }
                            Errors.Enqueue(stmt.Ids[0].ToString());
                            Errors.Enqueue("Wrong parameter(s).");
                        }
                        else if (stmt.Ids[0].Value == "warning")
                        {
                            if (stmt.Parameters.Count == 1)
                            {
                                if (stmt.Parameters[0].Type == t_str1 || stmt.Parameters[0].Type == t_str2)
                                {
                                    last_transition.Warning = stmt.Parameters[0].Value;
                                    break;
                                }
                            }
                            Errors.Enqueue(stmt.Ids[0].ToString());
                            Errors.Enqueue("Wrong parameter(s).");
                        }
                        else if (stmt.Ids[0].Value == "hint")
                        {
                            if (stmt.Parameters.Count == 1)
                            {
                                if (stmt.Parameters[0].Type == t_str1 || stmt.Parameters[0].Type == t_str2)
                                {
                                    last_transition.Hint = stmt.Parameters[0].Value;
                                    break;
                                }
                            }
                            Errors.Enqueue(stmt.Ids[0].ToString());
                            Errors.Enqueue("Wrong parameter(s).");
                        }
                        else
                        {
                            Errors.Enqueue(stmt.Ids[0].ToString());
                            Errors.Enqueue("No such function.");
                        }
                        break;
                    case StatementType.Final:
                        foreach (Token token in stmt.Parameters)
                        {
                            if (States.TryGetValue(token.Value, out State state))
                            {
                                state.IsFinal = true;
                            }
                            else
                            {
                                States.Add(token.Value, new State() { IsFinal = true });
                            }
                        }
                        break;
                    default:
                        break;
				}
            }
        }

        public static bool ProcessAll(string Program, Interface Interface,
                               out Dictionary<string, Set> Sets,
                               out Dictionary<string, State> States,
                               out Queue<string> Errors,
                               out Queue<string> Warnings)
        {
            List<Token> Tokens = ProcessLexical(Program, Interface);
            Errors = new Queue<string>();
            Warnings = new Queue<string>();

            if (Tokens == null)
            {
                Errors.Enqueue("Your program contains lexical error. please solve it before building.");
                States = null;
                Sets = null;
                return false;
            }

            foreach (Token token in Tokens)
            {
                if (token.IsMessage)
                {
                    if (token.ShouldPrint)
                    {
                        if (token.MessageType == TokenMessageType.Error)
                        {
                            Errors.Enqueue(token.ToString());
                        }
                        else
                        {
                            Warnings.Enqueue(token.ToString());
                        }
                    }
                }
            }

            if (Errors.Count > 0)
            {
                if (Errors.Count == 1)
                    Errors.Enqueue("Your program contains a lexical error. please solve it before building.");
                else
                    Errors.Enqueue("Your program contains lexical errors. please solve them before building.");
                States = null;
                Sets = null;
                return false;
            }

            ProcessSyntax(Tokens, out List<StatementInfo> Statements, out Errors);

			if (Errors.Count > 0)
			{
				if (Errors.Count == 1)
					Errors.Enqueue("Your program contains a syntax error. please solve it before building.");
				else
					Errors.Enqueue("Your program contains syntax errors. please solve them before building.");
				States = null;
				Sets = null;
				return false;
			}

            ProcessSemantic(Statements, out Sets, out States, out Errors);

			if (Errors.Count > 0)
			{
				if (Errors.Count == 1)
					Errors.Enqueue("Your program contains a semantic error. please solve it before building.");
				else
					Errors.Enqueue("Your program contains semantic errors. please solve them before building.");
				States = null;
				Sets = null;
				return false;
			}

            Interface.Print("Built successfully.");
            return true;
        }
    }

    /*public class StateMachineProgram
    {
        public StateMachineProgram()
        {
            
        }

		public StateMachineProgram(App Interface)
		{
            this.Interface = Interface;
		}

        static StateMachine ProgramBuilder; // much recursion!

        List<Token> program = null;

        App Interface = null;

		const string t_operator = "operator";
		//const string t_action = "action";
		const string t_id = "id";
		const string t_str1 = "str1";
		const string t_str2 = "str2";

        public string Program
        {
            set
            {
                if (ProgramBuilder == null)
                {
                    ProgramBuilder = CompileUtility.MakeLexicalAnalyzer();
                }

                program = ProgramBuilder.Process(value);
            }
        }

        string ProcessStr(string str)
        {
			str = str.Replace("\\n", "\n");
			str = str.Replace("\\t", "\t");
            str = str.Replace("\\", "");    // (else/default)

            return str;
        }

        public bool CompileToStateMachine(StateMachine StateMachine) //, bool ForceCompile = false)
        {
            if (program == null)
            {
                Interface.Print("No program!");
                return false;
            }

			bool hasLexicalError = false;
			foreach (Token token in program)
			{
				if (token.ShouldPrint)
				{
					Interface.Print(token.ToString());

					if (token.MessageType == TokenMessageType.Error)
                    {
                        Interface.Print(token.ToString());
                        hasLexicalError = true;
                    }
                    else if (token.ShouldPrint)
                    {
                        Interface.Print(token.ToString());
                        program.Remove(token);
                    }
                    else if (token.IsMessage)
                    {
                        program.Remove(token);
                    }
				}
			}
            if (hasLexicalError) { Interface.Print("The program contains lexical error(s)."); return false; }

            if (program.Count == 0)
            {
                Interface.Print("Empty program.");
                return false;
            }

            SyntaxUtility.Process(program, out List<StatementInfo> Statements, out Queue<string> Errors);

            if (Errors.Count == 0)
            {
                
            }
            else
            {
                foreach (string err in Errors)
                {
                    Interface.Print(err);
                }
                Interface.Print("The program contains syntax error(s).");
                return false;
            }

            /*int i = 0;
            bool HasAtLeastOneState = false;
            bool BuiltSuccessfully = true; // Will be set to false on semantic errors...

            Dictionary<string, Set> sets = new Dictionary<string, Set>();
            Dictionary<string, State> states = new Dictionary<string, State>();

            Transition last_transition = null;

            while (true)
            {
                if (i >= program.Count)
                {
                    if (HasAtLeastOneState)
                    {
                        if (BuiltSuccessfully)
                        {
                            // TODO: Apply sets and states to the machine.

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Interface.Print("The program must contain at least one state!");
                        return false;
                    }
                }

                try
                {
                    if (program[i].Type == t_id)
                    {
                        string tmp = t_id;

                        i++;
                        if (program[i].Type == t_operator)
                        {
                            if (program[i].Value == ",")
                            {
								State s1;
								if (!states.TryGetValue(tmp, out s1))
								{
									s1 = new State();
									states.Add(tmp, s1);
								}

                                last_transition = new Transition();

                                i++;
                                if (program[i].Type == t_id)
                                {
                                    if (program[i].Value == "else")
                                    {
                                        s1.ElseTransition = last_transition;
                                    }
                                    else if (sets.TryGetValue(program[i].Value, out Set tmp_useless_set))
                                    {
                                        last_transition.Input = program[i].Value;
                                        last_transition.InputFromSet = true;
                                        s1.Transitions.Add(last_transition);
                                    }
                                    else
                                    {
										Interface.Print(program[i].ToString());
										Interface.Print("The set '" + program[i].Value + "' does not exist.");
                                        BuiltSuccessfully = false; //s1.Transitions.Add(last_transition); NO!
									}
                                }
                                else if (program[i].Type == t_str1)
                                {
                                    last_transition.Input = ProcessStr(program[i].Value);
                                    last_transition.InputFromSet = false;
                                    s1.Transitions.Add(last_transition);
                                }
                                else if (program[i].Type == t_str2)
                                {
									last_transition.Input = ProcessStr(program[i].Value);
									last_transition.InputFromSet = false;
                                    s1.Transitions.Add(last_transition);
                                }
                                else if (program[i].Type == t_operator && program[i].Value == "{")
                                {
                                    // TODO: sets that are defined inside transitions.
                                    //
                                    s1.Transitions.Add(last_transition);
                                }
                                else
                                {
                                    Interface.Print(program[i].ToString());
                                    Interface.Print("Expected an id, a string or '{' after ','.");
                                    return false;
                                }

                                i++;
                                if (program[i].Type == t_operator && program[i].Value == "=>")
                                {
                                    i++;
                                    if (program[i].Type == t_id)
                                    {
                                        State s2;
										if (!states.TryGetValue(program[i].Value, out s2))
										{
                                            s2 = new State();
											states.Add(program[i].Value, s2);
										}
                                        last_transition.DestinationState = program[i].Value;
                                    }
                                    else if (program[i].Type == t_operator && program[i].Value == "-")
                                    {
                                        continue;
                                    }
                                    else
                                    {
										Interface.Print(program[i].ToString());
										Interface.Print("Expected an id or a function after '=>'.");
										return false;
                                    }
                                }
                                else
                                {
									Interface.Print(program[i].ToString());
									Interface.Print("Expected a '=>'.");
									return false;
                                }

                                s1.Transitions.Add(last_transition);
                            }
                            else if (program[i].Value == "=")
                            {
								Set s;
								if (!sets.TryGetValue(tmp, out s))
								{
									s.Items.Clear();
                                    s.Ranges.Clear();
								}
                                else
                                {
									s = new Set();
									sets.Add(tmp, s);
                                }

                                i++;
                                if (program[i].Type == t_operator && program[i].Value == "{")
                                {
                                    if (program[i + 1].Type == t_operator && program[i + 1].Value == "}")
                                    {
										Interface.Print(program[i].ToString());
										Interface.Print("The " + program[i].Value + " set is not defined.");
                                        BuiltSuccessfully = false;
                                        i += 2;
                                        continue;
                                    }

                                    do
                                    {
                                        i++;
                                        if (program[i].Type == t_id)
                                        {
											if (!sets.TryGetValue(program[i].Value, out Set tmp_set))
											{
                                                foreach (string item in tmp_set.Items)
                                                {
                                                    s.Items.Add(item);
                                                }
                                                foreach (Range range in tmp_set.Ranges)
                                                {
                                                    s.Ranges.Add(range);
                                                }
                                            }
											else
											{
												Interface.Print(program[i].ToString());
												Interface.Print("The " + program[i].Value + " set is not defined.");
                                                BuiltSuccessfully = false;
											}
                                        }
                                        else if (program[i].Type == t_str1 || program[i].Type == t_str2)
                                        {
                                            tmp = ProcessStr(program[i].Value);

                                            if (program[i + 1].Type == t_operator && program[i + 1].Value == ",")
                                            {
                                                if (program[i + 2].Type == t_operator && program[i + 2].Value == "...")
                                                {
                                                    if (tmp.Length != 1)
                                                    {
														Interface.Print(program[i].ToString());
														Interface.Print("A range can only be defined with a single character string.");
														return false;
                                                    }

                                                    i += 3;
                                                    if (program[i].Type == t_operator && program[i].Value == ",")
                                                    {
                                                        i++;
                                                        if (program[i].Type == t_str1 || program[i].Type == t_str2)
                                                        {
                                                            if (ProcessStr(program[i].Value).Length != 1)
															{
																Interface.Print(program[i].ToString());
																Interface.Print("A range can only be defined with a single character string.");
																return false;
															}
                                                            s.Ranges.Add(new Range(tmp[0], ProcessStr(program[i].Value)[0]));
                                                        }
                                                        else
                                                        {
															Interface.Print(program[i].ToString());
															Interface.Print("Expected a single character string here.");
															return false;
                                                        }
                                                    }
                                                    else
                                                    {
														Interface.Print(program[i].ToString());
														Interface.Print("Expected a ',' after '...'.");
														return false;
                                                    }
                                                }
                                                else
                                                {
                                                    s.Items.Add(tmp);
                                                }
                                            }
                                            else
                                            {
                                                s.Items.Add(tmp);
                                            }
                                        }
                                        else
                                        {
                                            Interface.Print(program[i].ToString());
                                            Interface.Print("Expected an id, or a string.");
                                            return false;
                                        }
                                        i++;
                                    }
                                    while (program[i].Type == t_operator && program[i].Value == ",");

                                    if (program[i].Type == t_operator && program[i].Value == "}")
                                    {
                                        i++;
                                        continue;
                                    }
                                    else
                                    {
										Interface.Print(program[i].ToString());
                                        Interface.Print("Expected '}'.");
										return false;
                                    }
                                }
                                else
                                {
                                    Interface.Print(program[i].ToString());
                                    Interface.Print("Expected a '{' after '='.");
                                    return false;
                                }
                            }
                            else
                            {
                                Interface.Print(program[i].ToString());
                                Interface.Print("Expected ',' or '=' after the id.");
                                return false;
                            }
                        }
                        else
                        {
                            Interface.Print(program[i].ToString());
                            Interface.Print("Expected an operator after the id.");
                            return false;
                        }
                    }
                    else if (program[i].Type == t_operator && program[i].Value == "-")
                    {
						i++;
						while (program[i].Type == t_operator && program[i].Value == "-")
						{
						    i++;
						}

                        if (program[i].Type == t_id)
                        {
                            string function_name = program[i].Value;
                            List<string> Parameters = new List<string>();
							i++;
							if (program[i].Type == t_operator && program[i].Value == "(")
							{
                                i++;
                                if (function_name == "return" || function_name == "clear" || function_name == "ignore")
                                {
                                    bool starts_with_offset = false;
                                    if (program[i].Type == t_operator && program[i].Value == "-")
                                    {
                                        starts_with_offset = true;
                                        i++;
                                    }
                                    if (program[i].Type == t_id)
                                    {
                                        if (int.TryParse(program[i].Value, out int offset))
                                        {
                                            i++;
                                            if (program[i].Type == t_operator && program[i].Value == ","
                                                && function_name != "clear" && function_name != "ignore")
                                            {
                                                i++;
                                                if (program[i].Type == t_str1 || program[i].Type == t_str2)
                                                {
                                                    i++;
                                                    if (program[i].Type == t_operator && program[i].Value == ")")
                                                    {
                                                        last_transition.Returns.Add(new Return(offset, ProcessStr(program[i - 1].Value)));
                                                        i++;
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        Interface.Print(program[i].ToString());
                                                        Interface.Print("Unknown format.");
                                                        BuiltSuccessfully = false;
                                                        i++;
                                                        while (!(program[i].Type == t_operator && program[i].Value == ")"))
                                                        {
                                                            i++;
                                                        }
                                                        i++;
                                                        continue;
                                                    }
                                                }
                                            }
                                            else if (program[i].Type == t_operator && program[i].Value == ")")
                                            {
                                                last_transition.Returns.Add(new Return(offset, null));
                                                i++;
                                                continue;
                                            }
                                            else
                                            {
                                                Interface.Print(program[i].ToString());
                                                Interface.Print("Unknown format.");
                                                BuiltSuccessfully = false;
                                                i++;
                                                while (!(program[i].Type == t_operator && program[i].Value == ")"))
                                                {
                                                    i++;
                                                }
                                                i++;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            Interface.Print(program[i].ToString());
                                            Interface.Print("Unknown parameter.");
                                            BuiltSuccessfully = false;
                                            i++;
                                            while (!(program[i].Type == t_operator && program[i].Value == ")"))
                                            {
                                                i++;
                                            }
                                            i++;
                                            continue;
                                        }
                                    }
                                    else if ((program[i].Type == t_str1 || program[i].Type == t_str2) && !starts_with_offset
                                            && function_name != "clear" && function_name != "ignore")
                                    {
                                        i++;
                                        if (program[i].Type == t_operator && program[i].Value == ")")
                                        {
                                            last_transition.Returns.Add(new Return(0, ProcessStr(program[i].Value)));
                                            i++;
                                            continue;
                                        }
                                        else
                                        {
											Interface.Print(program[i].ToString());
											Interface.Print("Unknown parameter.");
											BuiltSuccessfully = false;
											i++;
											while (!(program[i].Type == t_operator && program[i].Value == ")"))
											{
												i++;
											}
											i++;
											continue;
                                        }
                                    }
                                    else
                                    {
                                        Interface.Print(program[i].ToString());
                                        Interface.Print("Unknown parameter.");
                                        BuiltSuccessfully = false;
                                        i++;
                                        while (!(program[i].Type == t_operator && program[i].Value == ")"))
                                        {
                                            i++;
                                        }
                                        i++;
                                        continue;
                                    }
                                }
                                else if (function_name == "error" || function_name == "warning" || function_name == "hint")
                                {
                                    // TODO
                                }
                                else
                                {
									Interface.Print(program[i].ToString());
									Interface.Print("Function '" + program[i].Value + "' not found.");
									BuiltSuccessfully = false;
									i++;
									while (!(program[i].Type == t_operator && program[i].Value == ")"))
									{
										i++;
									}
									i++;
									continue;
                                }
							}
                            else
                            {
                                if (function_name == "return"|| function_name == "clear" || function_name == "ignore")
                                {
                                    last_transition.Returns.Add(new Return(0, null));
                                    continue;
                                }
                                else
                                {
									Interface.Print(program[i].ToString());
									Interface.Print("Function '" + program[i].Value + "' not found.");
									BuiltSuccessfully = false;
                                    continue;
                                }
                            }
                        }
                        else
                        {
							Interface.Print(program[i].ToString());
							Interface.Print("Expected a function name after '-'s.");
							return false;
                        }
					}
                    else
                    {
                        Interface.Print(program[i].ToString());
                        Interface.Print("Expected an id or a function.");
                        return false;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Interface.Print("Incomplete program.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets or sets a token from the program.
        /// </summary>
        public Token this[int Index]
        {
            get
            {
                return program[Index];
            }
        }
    }*/

    public class StateMachine
    {
        public StateMachine()
        {
            
        }

        public StateMachine(string Program)
        {
            Build(Program);
        }

		public StateMachine(Interface Interface)
		{
            this.Interface = Interface;
		}

		public StateMachine(Interface Interface, string Program)
		{
            this.Interface = Interface;
			Build(Program);
		}

        Dictionary<string, Set> Sets = new Dictionary<string, Set>();
        Dictionary<string, State> States = new Dictionary<string, State>(); // THERE SHOULD BE A COPY OF THE START STATE WITH THE NAME '$start'
        public Interface Interface = null;

		public void AddSet(string Name, Set Set)
		{
			Sets.Add(Name, Set);
		}

        public void AddState(string Name, State State)
        {
            States.Add(Name, State);
        }

        public void Clear()
        {
            Sets.Clear();
            States.Clear();
        }

        public bool Build(string Program)
        {
			if (CompileUtility.ProcessAll(Program, Interface,
										  out Dictionary<string, Set> Sets,
										  out Dictionary<string, State> States,
										  out Queue<string> Errors,
										  out Queue<string> Warnings))
			{
				this.Sets = Sets;
				this.States = States;

                if (Interface != null)
                {
                    foreach (string msg in Errors)
                    {
                        Interface.Print(msg);
                    }
					foreach (string msg in Warnings)
					{
						Interface.Print(msg);
					}
                }

                return true;
			}
            else
            {
				if (Interface != null)
				{
					foreach (string msg in Errors)
					{
						Interface.Print(msg);
					}
					foreach (string msg in Warnings)
					{
						Interface.Print(msg);
					}
				}

                return false;
            }
        }

        public bool Process(string Content, out List<Token> Tokens)
        {
            if (States.Count < 1)
            {
                Interface.Print("The state machine should at least have 1 state.");
                Tokens = null;
                return false;
            }

            if (States.TryGetValue("$start", out State C))
            {
                Tokens = new List<Token>();
                int x = 1;
                int y = 1;
                Queue<char> ReturnQueue = new Queue<char>();
                Content = Content.Replace("\r", ""); // TODO: Should I?!?! NO, so what?!

                for (int i = 0; i < Content.Length; i++)
                {
                    bool Accepted = false;
                    foreach (Transition tr in C.Transitions)
                    {
                        if (tr.Input == null)
                            continue;
                        int last_offset = 0;
                        if (tr.InputFromSet)
                        {
                            if (Sets.TryGetValue(tr.Input, out Set s))
                            {
                                foreach (string item in s.Items)
                                {
                                    int j = i;
                                    Accepted = true;
                                    foreach (char ch in item)
                                    {
                                        try
                                        {
                                            if (Content[j] == ch)
                                                j++;
                                            else
                                                Accepted = false;
                                        }
                                        catch (Exception)
                                        {
                                            Accepted = false;
                                        }
                                    }
									if (Accepted)
                                    {
                                        for (int k = i; k < j; k++)
                                        {
                                            ReturnQueue.Enqueue(Content[k]);
                                            last_offset++;
                                        }
                                        i = j - 1;
                                        break;
                                    }
                                }
                                if (!Accepted)
                                {
                                    foreach (Range range in s.Ranges)
                                    {
                                        if (Content[i] >= range.Min && Content[i] <= range.Max)
                                        {
                                            Accepted = true;
                                            ReturnQueue.Enqueue(Content[i]);
                                            last_offset++;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
								Interface.Print("Set not found. This is not your fault, please ask the developer to fix this bug!");
								return false;
                            }
                        }
                        else
                        {
							int j = i;
							Accepted = true;
							foreach (char ch in tr.Input)
							{
								try
								{
									if (Content[j] == ch)
										j++;
									else
										Accepted = false;
								}
								catch (Exception)
								{
									Accepted = false;
								}
							}
							if (Accepted == true)
                            {
								for (int k = i; k < j; k++)
								{
									ReturnQueue.Enqueue(Content[k]);
                                    last_offset++;
                                }
                                i = j - 1;
                            }
                        }
                        if (Accepted)
                        {
                            if (tr.Error != null)
                            {
                                Tokens.Add(Token.Error(i, x, y, tr.Error));
                            }
                            if (tr.Warning != null)
                            {
                                Tokens.Add(Token.Warning(i, x, y, tr.Warning));
                            }
                            if (tr.Hint != null)
                            {
                                Tokens.Add(Token.Hint(i, x, y, tr.Hint));
                            }
                            foreach (Return r in tr.Returns)
                            {
                                string v = "";
                                while (ReturnQueue.Count > (r.Offset == -1 ? last_offset : r.Offset))
                                {
                                    char ch = ReturnQueue.Dequeue();
                                    v += ch;

                                    x++;
                                    if (ch == '\n')
                                    {
                                        y++;
                                        x = 1;
                                    }
                                }
                                if (r.TypeName != null)
                                    Tokens.Add(Token.Return(i, x, y, r.TypeName, v));
                            }

                            if (tr.DestinationState != null)
                            {
                                if (!States.TryGetValue(tr.DestinationState, out C))
                                {
                                    Interface.Print("State not found. This is not your fault, please ask the developer to fix this bug!");
                                    return false;
                                }
                                break;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    if (!Accepted)
                    {
                        if (C.ElseTransition != null)
                        {
                            ReturnQueue.Enqueue(Content[i]);
                            if (C.ElseTransition.Error != null)
							{
								Tokens.Add(Token.Error(i, x, y, C.ElseTransition.Error));
                            }
							if (C.ElseTransition.Warning != null)
							{
                                Tokens.Add(Token.Warning(i, x, y, C.ElseTransition.Warning));
							}
							if (C.ElseTransition.Hint != null)
							{
                                Tokens.Add(Token.Hint(i, x, y, C.ElseTransition.Hint));
							}
                            foreach (Return r in C.ElseTransition.Returns)
                            {
								string v = "";
								while (ReturnQueue.Count > (r.Offset == -1 ? 1 : r.Offset))
								{
									char ch = ReturnQueue.Dequeue();
									v += ch;

									x++;
									if (ch == '\n')
									{
										y++;
										x = 1;
									}
								}
                                if (r.TypeName != null)
                                    Tokens.Add(Token.Return(i, x, y, r.TypeName, v));
                            }
                            if (C.ElseTransition.DestinationState != null)
                            {
                                if (!States.TryGetValue(C.ElseTransition.DestinationState, out C))
                                {
									Interface.Print("State not found. This is not your fault, please ask the developer to fix this bug!");
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Interface.Print("Unexpected illegal character at (line: " + y + ", col: " + x + ").");
                            return false;
                        }
                    }
                }

                foreach (Transition tr in C.Transitions)
                {
                    if (tr.Input == null)
                    {
                        if (tr.Error != null)
                        {
                            Tokens.Add(Token.Error(Content.Length, x, y, tr.Error));
                        }
                        if (tr.Warning != null)
                        {
                            Tokens.Add(Token.Warning(Content.Length, x, y, tr.Warning));
                        }
                        if (tr.Hint != null)
                        {
                            Tokens.Add(Token.Hint(Content.Length, x, y, tr.Hint));
                        }
                        foreach (Return r in tr.Returns)
                        {
                            string v = "";
                            while (ReturnQueue.Count > ((r.Offset == -1 || r.Offset == 0) ? 0 : (r.Offset - 1)))
                            {
                                char ch = ReturnQueue.Dequeue();
                                v += ch;

                                x++;
                                if (ch == '\n')
                                {
                                    y++;
                                    x = 1;
                                }
                            }
                            if (r.TypeName != null)
                                Tokens.Add(Token.Return(Content.Length, x, y, r.TypeName, v));
                        }
                        if (tr.DestinationState != null)
                        {
                            if (!States.TryGetValue(tr.DestinationState, out C))
                            {
                                Interface.Print("State not found. This is not your fault, please ask the developer to fix this bug!");
                                return false;
                            }
                        }
                        //else
                        //{
                        //    return false;
                        //}
                        break;
                    }
                }

                return C.IsFinal;
            }
            else
            {
                Interface.Print("No start state. This is not your fault, please ask the developer to fix this bug!");
                Tokens = null;
                return false;
            }
        }
    }
}
