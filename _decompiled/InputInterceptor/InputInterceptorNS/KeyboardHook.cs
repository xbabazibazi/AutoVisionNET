using System.Collections.Generic;
using System.Threading;

namespace InputInterceptorNS;

public class KeyboardHook : Hook<KeyStroke>
{
	private struct KeyData
	{
		public KeyCode Code;

		public bool Shift;
	}

	private static readonly Dictionary<char, KeyData> KeyDictionary;

	private static readonly KeyData QuestionMark;

	static KeyboardHook()
	{
		KeyDictionary = new Dictionary<char, KeyData>();
		KeyDictionary.Add('`', new KeyData
		{
			Code = KeyCode.Tilde
		});
		KeyDictionary.Add('1', new KeyData
		{
			Code = KeyCode.One
		});
		KeyDictionary.Add('2', new KeyData
		{
			Code = KeyCode.Two
		});
		KeyDictionary.Add('3', new KeyData
		{
			Code = KeyCode.Three
		});
		KeyDictionary.Add('4', new KeyData
		{
			Code = KeyCode.Four
		});
		KeyDictionary.Add('5', new KeyData
		{
			Code = KeyCode.Five
		});
		KeyDictionary.Add('6', new KeyData
		{
			Code = KeyCode.Six
		});
		KeyDictionary.Add('7', new KeyData
		{
			Code = KeyCode.Seven
		});
		KeyDictionary.Add('8', new KeyData
		{
			Code = KeyCode.Eight
		});
		KeyDictionary.Add('9', new KeyData
		{
			Code = KeyCode.Nine
		});
		KeyDictionary.Add('0', new KeyData
		{
			Code = KeyCode.Zero
		});
		KeyDictionary.Add('-', new KeyData
		{
			Code = KeyCode.Dash
		});
		KeyDictionary.Add('=', new KeyData
		{
			Code = KeyCode.Equals
		});
		KeyDictionary.Add('q', new KeyData
		{
			Code = KeyCode.Q
		});
		KeyDictionary.Add('w', new KeyData
		{
			Code = KeyCode.W
		});
		KeyDictionary.Add('e', new KeyData
		{
			Code = KeyCode.E
		});
		KeyDictionary.Add('r', new KeyData
		{
			Code = KeyCode.R
		});
		KeyDictionary.Add('t', new KeyData
		{
			Code = KeyCode.T
		});
		KeyDictionary.Add('y', new KeyData
		{
			Code = KeyCode.Y
		});
		KeyDictionary.Add('u', new KeyData
		{
			Code = KeyCode.U
		});
		KeyDictionary.Add('i', new KeyData
		{
			Code = KeyCode.I
		});
		KeyDictionary.Add('o', new KeyData
		{
			Code = KeyCode.O
		});
		KeyDictionary.Add('p', new KeyData
		{
			Code = KeyCode.P
		});
		KeyDictionary.Add('[', new KeyData
		{
			Code = KeyCode.OpenBracketBrace
		});
		KeyDictionary.Add(']', new KeyData
		{
			Code = KeyCode.CloseBracketBrace
		});
		KeyDictionary.Add('a', new KeyData
		{
			Code = KeyCode.A
		});
		KeyDictionary.Add('s', new KeyData
		{
			Code = KeyCode.S
		});
		KeyDictionary.Add('d', new KeyData
		{
			Code = KeyCode.D
		});
		KeyDictionary.Add('f', new KeyData
		{
			Code = KeyCode.F
		});
		KeyDictionary.Add('g', new KeyData
		{
			Code = KeyCode.G
		});
		KeyDictionary.Add('h', new KeyData
		{
			Code = KeyCode.H
		});
		KeyDictionary.Add('j', new KeyData
		{
			Code = KeyCode.J
		});
		KeyDictionary.Add('k', new KeyData
		{
			Code = KeyCode.K
		});
		KeyDictionary.Add('l', new KeyData
		{
			Code = KeyCode.L
		});
		KeyDictionary.Add(';', new KeyData
		{
			Code = KeyCode.Semicolon
		});
		KeyDictionary.Add('\'', new KeyData
		{
			Code = KeyCode.Apostrophe
		});
		KeyDictionary.Add('\\', new KeyData
		{
			Code = KeyCode.Backslash
		});
		KeyDictionary.Add('z', new KeyData
		{
			Code = KeyCode.Z
		});
		KeyDictionary.Add('x', new KeyData
		{
			Code = KeyCode.X
		});
		KeyDictionary.Add('c', new KeyData
		{
			Code = KeyCode.C
		});
		KeyDictionary.Add('v', new KeyData
		{
			Code = KeyCode.V
		});
		KeyDictionary.Add('b', new KeyData
		{
			Code = KeyCode.B
		});
		KeyDictionary.Add('n', new KeyData
		{
			Code = KeyCode.N
		});
		KeyDictionary.Add('m', new KeyData
		{
			Code = KeyCode.M
		});
		KeyDictionary.Add(',', new KeyData
		{
			Code = KeyCode.Comma
		});
		KeyDictionary.Add('.', new KeyData
		{
			Code = KeyCode.Dot
		});
		KeyDictionary.Add('/', new KeyData
		{
			Code = KeyCode.Slash
		});
		KeyDictionary.Add(' ', new KeyData
		{
			Code = KeyCode.Space
		});
		KeyDictionary.Add('~', new KeyData
		{
			Code = KeyCode.Tilde,
			Shift = true
		});
		KeyDictionary.Add('!', new KeyData
		{
			Code = KeyCode.One,
			Shift = true
		});
		KeyDictionary.Add('@', new KeyData
		{
			Code = KeyCode.Two,
			Shift = true
		});
		KeyDictionary.Add('#', new KeyData
		{
			Code = KeyCode.Three,
			Shift = true
		});
		KeyDictionary.Add('$', new KeyData
		{
			Code = KeyCode.Four,
			Shift = true
		});
		KeyDictionary.Add('%', new KeyData
		{
			Code = KeyCode.Five,
			Shift = true
		});
		KeyDictionary.Add('^', new KeyData
		{
			Code = KeyCode.Six,
			Shift = true
		});
		KeyDictionary.Add('&', new KeyData
		{
			Code = KeyCode.Seven,
			Shift = true
		});
		KeyDictionary.Add('*', new KeyData
		{
			Code = KeyCode.Eight,
			Shift = true
		});
		KeyDictionary.Add('(', new KeyData
		{
			Code = KeyCode.Nine,
			Shift = true
		});
		KeyDictionary.Add(')', new KeyData
		{
			Code = KeyCode.Zero,
			Shift = true
		});
		KeyDictionary.Add('_', new KeyData
		{
			Code = KeyCode.Dash,
			Shift = true
		});
		KeyDictionary.Add('+', new KeyData
		{
			Code = KeyCode.Equals,
			Shift = true
		});
		KeyDictionary.Add('Q', new KeyData
		{
			Code = KeyCode.Q,
			Shift = true
		});
		KeyDictionary.Add('W', new KeyData
		{
			Code = KeyCode.W,
			Shift = true
		});
		KeyDictionary.Add('E', new KeyData
		{
			Code = KeyCode.E,
			Shift = true
		});
		KeyDictionary.Add('R', new KeyData
		{
			Code = KeyCode.R,
			Shift = true
		});
		KeyDictionary.Add('T', new KeyData
		{
			Code = KeyCode.T,
			Shift = true
		});
		KeyDictionary.Add('Y', new KeyData
		{
			Code = KeyCode.Y,
			Shift = true
		});
		KeyDictionary.Add('U', new KeyData
		{
			Code = KeyCode.U,
			Shift = true
		});
		KeyDictionary.Add('I', new KeyData
		{
			Code = KeyCode.I,
			Shift = true
		});
		KeyDictionary.Add('O', new KeyData
		{
			Code = KeyCode.O,
			Shift = true
		});
		KeyDictionary.Add('P', new KeyData
		{
			Code = KeyCode.P,
			Shift = true
		});
		KeyDictionary.Add('{', new KeyData
		{
			Code = KeyCode.OpenBracketBrace,
			Shift = true
		});
		KeyDictionary.Add('}', new KeyData
		{
			Code = KeyCode.CloseBracketBrace,
			Shift = true
		});
		KeyDictionary.Add('A', new KeyData
		{
			Code = KeyCode.A,
			Shift = true
		});
		KeyDictionary.Add('S', new KeyData
		{
			Code = KeyCode.S,
			Shift = true
		});
		KeyDictionary.Add('D', new KeyData
		{
			Code = KeyCode.D,
			Shift = true
		});
		KeyDictionary.Add('F', new KeyData
		{
			Code = KeyCode.F,
			Shift = true
		});
		KeyDictionary.Add('G', new KeyData
		{
			Code = KeyCode.G,
			Shift = true
		});
		KeyDictionary.Add('H', new KeyData
		{
			Code = KeyCode.H,
			Shift = true
		});
		KeyDictionary.Add('J', new KeyData
		{
			Code = KeyCode.J,
			Shift = true
		});
		KeyDictionary.Add('K', new KeyData
		{
			Code = KeyCode.K,
			Shift = true
		});
		KeyDictionary.Add('L', new KeyData
		{
			Code = KeyCode.L,
			Shift = true
		});
		KeyDictionary.Add(':', new KeyData
		{
			Code = KeyCode.Semicolon,
			Shift = true
		});
		KeyDictionary.Add('"', new KeyData
		{
			Code = KeyCode.Apostrophe,
			Shift = true
		});
		KeyDictionary.Add('|', new KeyData
		{
			Code = KeyCode.Backslash,
			Shift = true
		});
		KeyDictionary.Add('Z', new KeyData
		{
			Code = KeyCode.Z,
			Shift = true
		});
		KeyDictionary.Add('X', new KeyData
		{
			Code = KeyCode.X,
			Shift = true
		});
		KeyDictionary.Add('C', new KeyData
		{
			Code = KeyCode.C,
			Shift = true
		});
		KeyDictionary.Add('V', new KeyData
		{
			Code = KeyCode.V,
			Shift = true
		});
		KeyDictionary.Add('B', new KeyData
		{
			Code = KeyCode.B,
			Shift = true
		});
		KeyDictionary.Add('N', new KeyData
		{
			Code = KeyCode.N,
			Shift = true
		});
		KeyDictionary.Add('M', new KeyData
		{
			Code = KeyCode.M,
			Shift = true
		});
		KeyDictionary.Add('<', new KeyData
		{
			Code = KeyCode.Comma,
			Shift = true
		});
		KeyDictionary.Add('>', new KeyData
		{
			Code = KeyCode.Dot,
			Shift = true
		});
		KeyDictionary.Add('?', new KeyData
		{
			Code = KeyCode.Slash,
			Shift = true
		});
		QuestionMark = new KeyData
		{
			Code = KeyCode.Slash,
			Shift = true
		};
	}

	public KeyboardHook(KeyboardFilter filter = KeyboardFilter.None, CallbackAction callback = null)
		: base((ushort)filter, (Predicate)InputInterceptor.IsKeyboard, callback)
	{
	}

	public KeyboardHook(CallbackAction callback)
		: base((ushort)255, (Predicate)InputInterceptor.IsKeyboard, callback)
	{
	}

	protected override void CallbackWrapper(ref Stroke stroke)
	{
		base.Callback(ref stroke.Key);
	}

	public bool SetKeyState(KeyCode code, KeyState state)
	{
		if (base.CanSimulateInput)
		{
			Stroke stroke = new Stroke
			{
				Key = 
				{
					Code = code,
					State = state
				}
			};
			return InputInterceptor.Send(base.Context, base.AnyDevice, ref stroke, 1u) == 1;
		}
		return false;
	}

	public bool SimulateKeyDown(KeyCode code)
	{
		return SetKeyState(code, KeyState.Down);
	}

	public bool SimulateKeyUp(KeyCode code)
	{
		return SetKeyState(code, KeyState.Up);
	}

	public bool SimulateKeyPress(KeyCode code, int releaseDelay = 75)
	{
		if (SimulateKeyDown(code))
		{
			Thread.Sleep(releaseDelay);
			return SimulateKeyUp(code);
		}
		return false;
	}

	public bool SimulateInput(string text, int delayBetweenKeyPresses = 50, int releaseDelay = 75)
	{
		bool flag = false;
		foreach (char key in text)
		{
			if (!KeyDictionary.TryGetValue(key, out var value))
			{
				value = QuestionMark;
			}
			if (value.Shift != flag)
			{
				if (value.Shift)
				{
					if (!SetKeyState(KeyCode.LeftShift, KeyState.Down))
					{
						return false;
					}
				}
				else if (!SetKeyState(KeyCode.LeftShift, KeyState.Up))
				{
					return false;
				}
				flag = value.Shift;
			}
			if (!SimulateKeyPress(value.Code, releaseDelay))
			{
				return false;
			}
			Thread.Sleep(delayBetweenKeyPresses);
		}
		if (flag && !SetKeyState(KeyCode.LeftShift, KeyState.Up))
		{
			return false;
		}
		return true;
	}
}
