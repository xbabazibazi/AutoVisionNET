using System;
using System.Threading;
using System.Threading.Tasks;
using InputInterceptorNS;
using InputManager;
using SettingsManager;
using SimpleLogger;
using UI2.Interfaces;

namespace UI2.Services;

public class AttackService(InputUtils inputUtils, Logger logger) : IAttack
{
	private CancellationTokenSource _cts = new CancellationTokenSource();

	private bool _isCtsDisposed = false;

	private static readonly Random Random = new Random();

	private readonly Settings settings = Settings.Instance;

	private int SkillDelay { get; set; }

	private int RDelay { get; set; }

	private bool AttackHasSkill { get; set; }

	private bool AttackHasR { get; set; }

	private bool AttackHasZ { get; set; }

	private bool AttackHasNine { get; set; }

	private bool AttackHasEight { get; set; }

	private bool IsKeyPressed { get; set; }

	private bool IsGenieStarted { get; set; }

	private bool IsAttackStarted { get; set; }

	private void StartAttack()
	{
		UpdateAttackSettings();
		if (!AttackHasSkill && !AttackHasR && !AttackHasZ && !AttackHasEight && !AttackHasNine)
		{
			return;
		}
		if (AttackHasZ)
		{
			Task.Run(() => StartZTask(_cts.Token), _cts.Token);
		}
		if (AttackHasR)
		{
			Task.Run(() => StartRTask(_cts.Token), _cts.Token);
		}
		if (AttackHasSkill)
		{
			Task.Run(() => StartSkillAttack(_cts.Token), _cts.Token);
		}
		if (AttackHasNine)
		{
			Task.Run(() => StartNineAttack(_cts.Token), _cts.Token);
		}
		if (AttackHasEight)
		{
			Task.Run(() => StartEightAttack(_cts.Token), _cts.Token);
		}
	}

	private async Task StartZTask(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			for (int i = 0; i < 3; i++)
			{
				inputUtils.SimulateKeyPress(KeyCode.Z, 53);
				await Task.Delay(53, token);
			}
		}
	}

	private async Task StartRTask(CancellationToken token)
	{
		int delay = 50;
		while (!token.IsCancellationRequested)
		{
			if (settings.Macro.Attack.IsRandomDelay)
			{
				delay = Random.Next(50, 101);
			}
			inputUtils.SimulateKeyPress(KeyCode.R, delay);
			await Task.Delay(RDelay, token);
		}
	}

	private async Task StartSkillAttack(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			inputUtils.SimulateKeyPress(KeyCode.Zero, SkillDelay);
			await Task.Delay(SkillDelay, token);
		}
	}

	private async Task StartNineAttack(CancellationToken token)
	{
		int delay = 50;
		while (!token.IsCancellationRequested)
		{
			if (settings.Macro.Attack.IsRandomDelay)
			{
				delay = Random.Next(50, 101);
			}
			inputUtils.SimulateKeyPress(KeyCode.Nine, delay);
			await Task.Delay(50, token);
		}
	}

	private async Task StartEightAttack(CancellationToken token)
	{
		int delay = 50;
		while (!token.IsCancellationRequested)
		{
			if (settings.Macro.Attack.IsRandomDelay)
			{
				delay = Random.Next(50, 101);
			}
			inputUtils.SimulateKeyPress(KeyCode.Eight, delay);
			await Task.Delay(50, token);
		}
	}

	private void ResetCancellationToken()
	{
		if (_cts != null && !_isCtsDisposed)
		{
			try
			{
				_cts.Cancel();
				_cts.Dispose();
			}
			catch (ObjectDisposedException)
			{
				logger.LogWarning("ResetCancellationToken: CTS zaten dispose edilmiş.");
			}
		}
		_cts = new CancellationTokenSource();
		_isCtsDisposed = false;
	}

	private void ToggleAttack()
	{
		if (IsGenieStarted)
		{
			if (!IsAttackStarted)
			{
				ResetCancellationToken();
				StartAttack();
				IsAttackStarted = true;
				logger.LogInformation("Attack started by Genie.");
			}
		}
		else if (IsKeyPressed)
		{
			if (!IsAttackStarted)
			{
				ResetCancellationToken();
				StartAttack();
				IsAttackStarted = true;
				logger.LogInformation("Attack started manually.");
			}
		}
		else
		{
			if (!IsAttackStarted)
			{
				return;
			}
			if (_cts != null && !_isCtsDisposed)
			{
				try
				{
					_cts.Cancel();
					_cts.Dispose();
					_isCtsDisposed = true;
				}
				catch (ObjectDisposedException)
				{
					logger.LogWarning("ToggleAttack: CTS zaten dispose edilmiş.");
				}
			}
			IsAttackStarted = false;
			logger.LogInformation("Attack stopped manually.");
		}
	}

	public void ToggleGenieStarted(bool status)
	{
		try
		{
			IsGenieStarted = status;
			if (IsGenieStarted)
			{
				ResetCancellationToken();
				StartAttack();
				IsAttackStarted = true;
				logger.LogInformation("Attack started by Genie.");
				return;
			}
			if (_cts != null && !_isCtsDisposed)
			{
				try
				{
					_cts.Cancel();
					_cts.Dispose();
					_isCtsDisposed = true;
				}
				catch (ObjectDisposedException)
				{
					logger.LogWarning("ToggleGenieStarted: CTS zaten dispose edilmiş.");
				}
			}
			IsAttackStarted = false;
			logger.LogInformation("Attack stopped because Genie is off.");
			if (IsKeyPressed)
			{
				ToggleAttack();
			}
		}
		catch (Exception ex2)
		{
			logger.LogError("ToggleGenieStarted hatası: " + ex2.Message, ex2);
		}
	}

	public void ToggleKeyPressed()
	{
		IsKeyPressed = !IsKeyPressed;
		if (IsGenieStarted && !IsKeyPressed)
		{
			if (_cts != null && !_isCtsDisposed)
			{
				try
				{
					_cts.Cancel();
					_cts.Dispose();
					_isCtsDisposed = true;
				}
				catch (ObjectDisposedException)
				{
					logger.LogWarning("ToggleKeyPressed: CTS zaten dispose edilmiş.");
				}
			}
			IsAttackStarted = false;
			logger.LogInformation("Attack stopped manually while Genie is on.");
		}
		else
		{
			ToggleAttack();
		}
	}

	private void UpdateAttackSettings()
	{
		AttackHasSkill = settings.Macro.Attack.HasSkill;
		AttackHasR = settings.Macro.Attack.HasR;
		AttackHasZ = settings.Macro.Attack.HasZ;
		AttackHasEight = settings.Macro.Attack.HasEight;
		AttackHasNine = settings.Macro.Attack.HasNine;
		SkillDelay = (int)settings.Macro.Attack.Delay;
		RDelay = (int)settings.Macro.Attack.RDelay;
	}
}
