using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InputInterceptorNS;
using InputManager;
using SettingsManager;
using UI2.ScreenCapture;
using UI2.Services;

namespace UI2;

public class KeyCommandManager
{
	private readonly Dictionary<(KeyCode Key, bool Control), Func<Task>> _commands;

	// The underlying hook reports repeated KeyState.Down events while a key is held (normal OS
	// key-repeat), and Execute() never awaited its command - holding Ctrl+D for even a moment
	// spawned several overlapping TPParty() runs whose F1/Tab/skill presses interleaved and
	// stomped on each other's target selection, degrading several intended TPs into what looked
	// like just one. This tracks which keys currently have a command in flight and ignores
	// repeats until it finishes.
	private readonly HashSet<(KeyCode Key, bool Control)> _running = new HashSet<(KeyCode, bool)>();

	private readonly AttackService _attackService;

	private readonly InputUtils _inputUtils;

	private readonly Settings _settings;

	private readonly ScreenCaptureMainForm _screenCaptureMainForm;

	private readonly Form1 _mainForm;

	public KeyCommandManager(AttackService attackService, InputUtils inputUtils, Settings settings, ScreenCaptureMainForm screenCaptureMainForm, Form1 mainForm)
	{
		_commands = new Dictionary<(KeyCode, bool), Func<Task>>();
		_attackService = attackService;
		_inputUtils = inputUtils;
		_settings = settings;
		_screenCaptureMainForm = screenCaptureMainForm;
		_mainForm = mainForm;
		RegisterAllCommands();
	}

	private void RegisterAllCommands()
	{
		Register(KeyCode.A, control: true, delegate
		{
			_attackService.ToggleKeyPressed();
			return Task.CompletedTask;
		});
		Register(KeyCode.NumLock, control: false, () => _inputUtils.StartLoginAsync());
		Register(KeyCode.Z, control: true, () => _inputUtils.IWill());
		Register(KeyCode.X, control: true, () => _inputUtils.ILove());
		Register(KeyCode.D, control: true, () => _inputUtils.TPParty((int)_settings.Macro.TpParty.PartyMemberCount, (int)_settings.Macro.TpParty.TPDelay, _settings.Macro.TpParty.ResistOnTp));
		Register(KeyCode.F11, control: false, delegate
		{
			ToggleAllFormsVisibility();
			return Task.CompletedTask;
		});
		Register(KeyCode.W, control: true, delegate
		{
			_screenCaptureMainForm.Alarm.StopAlarm();
			return Task.CompletedTask;
		});
	}

	private void ToggleAllFormsVisibility()
	{
		try
		{
			List<Form> list = new List<Form>(Application.OpenForms.Cast<Form>());
			foreach (Form item in list)
			{
				if (item != _mainForm && !(item is RectangleOverlayForm) && !item.IsDisposed)
				{
					item.Visible = false;
				}
			}
			_mainForm.Visible = !_mainForm.Visible;
		}
		catch (Exception ex)
		{
			_mainForm.ShowError("Formlar gizlenirken hata oluştu: " + ex.Message);
		}
	}

	public void Register(KeyCode key, bool control, Func<Task> command)
	{
		_commands[(key, control)] = command;
	}

	public bool Execute(KeyCode key, bool control)
	{
		if (!_commands.TryGetValue((key, control), out Func<Task> value))
		{
			return false;
		}
		var commandKey = (key, control);
		lock (_running)
		{
			if (!_running.Add(commandKey))
			{
				return true;
			}
		}
		RunAndRelease(commandKey, value);
		return true;
	}

	private async void RunAndRelease((KeyCode Key, bool Control) commandKey, Func<Task> command)
	{
		try
		{
			await command();
		}
		finally
		{
			lock (_running)
			{
				_running.Remove(commandKey);
			}
		}
	}
}
