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

	// This table assumes the target machine's active Windows keyboard layout is Turkish Q (the
	// standard "TR" layout) - confirmed character-by-character against Microsoft's official
	// Turkish Q layout reference. Scan codes are physical key positions; what character a given
	// physical key produces depends entirely on the OS's active layout, not on this process, so
	// this table is only correct when that layout really is Turkish Q.
	//
	// A handful of EN-layout symbols have no safe Turkish Q equivalent and are deliberately left
	// out (they fall through to QuestionMark instead of risking the wrong character):
	//   @ # $ £ € ₺  - require AltGr, which SimulateInput has no way to hold down.
	//   ^ ~ ` ´ ¨    - Turkish Q puts these on dead keys that wait for and merge with the next
	//                  keystroke (e.g. to compose â, ê); sending one blind would silently corrupt
	//                  whatever character follows it, which is worse than typing nothing.
	//   < > [ ] { } \ | - sit on the extra ISO key or behind AltGr that this scan-code table has
	//                  no entry for at all.
	static KeyboardHook()
	{
		KeyDictionary = new Dictionary<char, KeyData>();
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
		// Turkish Q: the Dash key unshifted is '*', not '-'; the Equals key unshifted is '-', not
		// '='. The two are effectively swapped relative to English.
		KeyDictionary.Add('-', new KeyData
		{
			Code = KeyCode.Equals
		});
		KeyDictionary.Add('*', new KeyData
		{
			Code = KeyCode.Dash
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
		// Turkish Q: the physical "I" key types dotless 'ı' unshifted - dotted lowercase 'i' sits
		// on the physical key English calls apostrophe (').
		KeyDictionary.Add('i', new KeyData
		{
			Code = KeyCode.Apostrophe
		});
		KeyDictionary.Add('o', new KeyData
		{
			Code = KeyCode.O
		});
		KeyDictionary.Add('p', new KeyData
		{
			Code = KeyCode.P
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
		// Turkish Q: the Semicolon key unshifted is 'ş'; literal ';' lives on the ISO extra key
		// (the enum calls it Backslash - on Turkish ISO hardware it sits after L, not after ]),
		// shifted. The Apostrophe key unshifted is dotted 'i' (see above); literal "'" lives on
		// the Two key, shifted.
		KeyDictionary.Add(';', new KeyData
		{
			Code = KeyCode.Backslash,
			Shift = true
		});
		KeyDictionary.Add('\'', new KeyData
		{
			Code = KeyCode.Two,
			Shift = true
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
		// Turkish Q: the Comma key unshifted is 'ö', the Dot key unshifted is 'ç'. Literal ',' and
		// '.' live on the ISO extra key (unshifted) and the Slash key (unshifted) respectively.
		KeyDictionary.Add(',', new KeyData
		{
			Code = KeyCode.Backslash
		});
		KeyDictionary.Add('.', new KeyData
		{
			Code = KeyCode.Slash
		});
		// Literal '/' moved to the Seven key, shifted (Seven unshifted is still '7').
		KeyDictionary.Add('/', new KeyData
		{
			Code = KeyCode.Seven,
			Shift = true
		});
		KeyDictionary.Add(' ', new KeyData
		{
			Code = KeyCode.Space
		});
		KeyDictionary.Add('!', new KeyData
		{
			Code = KeyCode.One,
			Shift = true
		});
		KeyDictionary.Add('%', new KeyData
		{
			Code = KeyCode.Five,
			Shift = true
		});
		// Turkish Q: Seven unshifted was already claimed by '/' above, '&' moves to Six shifted
		// (Six unshifted is still '6').
		KeyDictionary.Add('&', new KeyData
		{
			Code = KeyCode.Six,
			Shift = true
		});
		KeyDictionary.Add('(', new KeyData
		{
			Code = KeyCode.Eight,
			Shift = true
		});
		KeyDictionary.Add(')', new KeyData
		{
			Code = KeyCode.Nine,
			Shift = true
		});
		KeyDictionary.Add('_', new KeyData
		{
			Code = KeyCode.Dash,
			Shift = true
		});
		KeyDictionary.Add('+', new KeyData
		{
			Code = KeyCode.Four,
			Shift = true
		});
		// Turkish Q: '=' moves to the Zero key, shifted (Equals key itself unshifted is now '-',
		// see above).
		KeyDictionary.Add('=', new KeyData
		{
			Code = KeyCode.Zero,
			Shift = true
		});
		// Turkish Q: '?' moves to the Equals key, shifted (Slash key's shifted state is ':', see
		// below) - also doubles as the fallback for any character this table doesn't cover.
		KeyDictionary.Add('?', new KeyData
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
		// Turkish Q: literal ':' lives on the Slash key, shifted; literal '"' lives on the Tilde
		// key (the key left of 1), unshifted - that key has no safe English equivalent anyway.
		KeyDictionary.Add(':', new KeyData
		{
			Code = KeyCode.Slash,
			Shift = true
		});
		KeyDictionary.Add('"', new KeyData
		{
			Code = KeyCode.Tilde
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
		// Used when a character has no entry above (would otherwise throw) - same key as the
		// literal '?' mapped earlier, so an unsupported character visibly becomes a question mark
		// rather than silently vanishing or typing something misleading.
		QuestionMark = new KeyData
		{
			Code = KeyCode.Equals,
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

	/// <summary>
	/// Whether SimulateInput knows a real key for this character (as opposed to silently falling
	/// back to typing '?'). Lets a caller warn the operator about an unsupported character before
	/// it reaches the game as the wrong text, instead of after.
	/// </summary>
	public static bool IsCharacterSupported(char key)
	{
		return KeyDictionary.ContainsKey(key);
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
