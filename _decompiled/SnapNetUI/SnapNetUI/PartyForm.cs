using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SnapNetUI;

public class PartyForm : Form
{
	private const int SlotCount = 8;

	private const string NoSelection = "— Seç —";

	private readonly Server _server;

	private readonly ComboBox[] _slotCombos = new ComboBox[SlotCount];

	private readonly RadioButton[] _slotLeaders = new RadioButton[SlotCount];

	private readonly Label lblStatus;

	private readonly Button btnFormParty;

	public PartyForm(Server server)
	{
		_server = server ?? throw new ArgumentNullException("server");

		Text = "Parti Oluştur";
		FormBorderStyle = FormBorderStyle.None;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = Color.FromArgb(28, 28, 33);
		ForeColor = Color.FromArgb(235, 235, 240);
		Font = new Font("Segoe UI", 9f);
		ClientSize = new Size(420, 520);

		Panel headerPanel = new Panel
		{
			Dock = DockStyle.Top,
			Height = 30,
			BackColor = Color.FromArgb(38, 38, 45)
		};
		Label lblTitle = new Label
		{
			Text = "Parti Oluştur (8 Kişi)",
			AutoSize = true,
			Font = AppFonts.Header(13f),
			ForeColor = Color.FromArgb(235, 235, 240),
			Location = new Point(10, 8)
		};
		Button btnHide = new Button
		{
			Text = "✕",
			FlatStyle = FlatStyle.Flat,
			ForeColor = Color.FromArgb(235, 235, 240),
			BackColor = Color.Transparent,
			Font = new Font("Segoe UI", 9f, FontStyle.Bold),
			Size = new Size(20, 20),
			Location = new Point(390, 5),
			Anchor = AnchorStyles.Top | AnchorStyles.Right
		};
		btnHide.FlatAppearance.BorderSize = 0;
		btnHide.Click += delegate { Hide(); };
		btnHide.MouseEnter += delegate { btnHide.BackColor = Color.FromArgb(200, 60, 60); };
		btnHide.MouseLeave += delegate { btnHide.BackColor = Color.Transparent; };
		headerPanel.Controls.Add(lblTitle);
		headerPanel.Controls.Add(btnHide);

		Panel body = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(28, 28, 33)
		};

		Label lblLeaderHint = new Label
		{
			Text = "Kurucu: parti daveti gönderecek karakter",
			AutoSize = true,
			ForeColor = Color.FromArgb(150, 155, 165),
			Font = new Font("Segoe UI", 8f),
			Location = new Point(14, 6)
		};
		body.Controls.Add(lblLeaderHint);

		int rowStart = 30;
		for (int i = 0; i < SlotCount; i++)
		{
			int y = rowStart + i * 38;
			Label lblSlot = new Label
			{
				Text = (i + 1) + ".",
				AutoSize = true,
				ForeColor = Color.FromArgb(180, 185, 195),
				Location = new Point(14, y + 6)
			};
			ComboBox combo = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(48, 48, 55),
				ForeColor = Color.White,
				Location = new Point(38, y),
				Size = new Size(220, 26)
			};
			RadioButton leader = new RadioButton
			{
				Text = "Kurucu",
				Appearance = Appearance.Button,
				FlatStyle = FlatStyle.Flat,
				BackColor = Color.FromArgb(60, 60, 65),
				ForeColor = Color.White,
				TextAlign = ContentAlignment.MiddleCenter,
				Location = new Point(268, y),
				Size = new Size(92, 26)
			};
			leader.FlatAppearance.BorderSize = 0;
			leader.FlatAppearance.CheckedBackColor = Color.FromArgb(0, 122, 204);
			body.Controls.Add(lblSlot);
			body.Controls.Add(combo);
			body.Controls.Add(leader);
			_slotCombos[i] = combo;
			_slotLeaders[i] = leader;
		}

		int actionY = rowStart + SlotCount * 38 + 10;
		btnFormParty = new Button
		{
			Text = "PARTİ KUR",
			Location = new Point(38, actionY),
			Size = new Size(322, 38),
			BackColor = Color.FromArgb(0, 153, 102),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
		};
		btnFormParty.FlatAppearance.BorderSize = 0;
		btnFormParty.Click += BtnFormParty_Click;
		body.Controls.Add(btnFormParty);

		lblStatus = new Label
		{
			Location = new Point(38, actionY + 46),
			Size = new Size(322, 40),
			ForeColor = Color.FromArgb(180, 185, 195),
			Font = new Font("Segoe UI", 8.5f)
		};
		body.Controls.Add(lblStatus);

		Controls.Add(body);
		Controls.Add(headerPanel);

		_server.ClientCountChanged += OnClientsChanged;
		_server.ClientStatusUpdated += OnClientsChanged;
		VisibleChanged += delegate
		{
			if (Visible)
			{
				RefreshAvailableClients();
			}
		};
		FormClosed += delegate
		{
			_server.ClientCountChanged -= OnClientsChanged;
			_server.ClientStatusUpdated -= OnClientsChanged;
		};
	}

	private void OnClientsChanged(int _)
	{
		SafeInvoke(RefreshAvailableClients);
	}

	private void OnClientsChanged()
	{
		SafeInvoke(RefreshAvailableClients);
	}

	private void SafeInvoke(Action action)
	{
		if (IsDisposed)
		{
			return;
		}
		if (InvokeRequired)
		{
			BeginInvoke(action);
		}
		else
		{
			action();
		}
	}

	private void RefreshAvailableClients()
	{
		string[] connected = _server.GetClientList().Select(c => c.nickname).OrderBy(n => n).ToArray();
		for (int i = 0; i < SlotCount; i++)
		{
			ComboBox combo = _slotCombos[i];
			string previous = combo.SelectedItem as string;
			combo.BeginUpdate();
			combo.Items.Clear();
			combo.Items.Add(NoSelection);
			combo.Items.AddRange(connected);
			bool stillConnected = previous != null && Array.IndexOf(connected, previous) >= 0;
			combo.SelectedItem = stillConnected ? previous : NoSelection;
			combo.EndUpdate();
			if (!stillConnected)
			{
				_slotLeaders[i].Checked = false;
			}
		}
	}

	private async void BtnFormParty_Click(object sender, EventArgs e)
	{
		int leaderIndex = Array.FindIndex(_slotLeaders, r => r.Checked);
		if (leaderIndex < 0)
		{
			ShowStatus("Lütfen bir kurucu seçin.", isError: true);
			return;
		}
		string leaderNickname = _slotCombos[leaderIndex].SelectedItem as string;
		if (string.IsNullOrEmpty(leaderNickname) || leaderNickname == NoSelection)
		{
			ShowStatus("Kurucu için bir karakter seçin.", isError: true);
			return;
		}
		string[] members = _slotCombos
			.Select(c => c.SelectedItem as string)
			.Where(n => !string.IsNullOrEmpty(n) && n != NoSelection && n != leaderNickname)
			.Distinct()
			.ToArray();
		if (members.Length == 0)
		{
			ShowStatus("En az 1 parti üyesi seçmelisiniz.", isError: true);
			return;
		}
		btnFormParty.Enabled = false;
		try
		{
			bool sent = await _server.SendCommandToClientAsync(leaderNickname, "PARTY_FORM:" + string.Join(",", members));
			ShowStatus(sent
				? $"Komut gönderildi: {leaderNickname}, {members.Length} kişiyi davet edecek."
				: leaderNickname + " artık bağlı değil.", isError: !sent);
		}
		finally
		{
			btnFormParty.Enabled = true;
		}
	}

	private void ShowStatus(string text, bool isError)
	{
		lblStatus.Text = text;
		lblStatus.ForeColor = isError ? Color.FromArgb(220, 120, 120) : Color.FromArgb(130, 200, 130);
	}

	public void ResetSelections()
	{
		SafeInvoke(delegate
		{
			foreach (ComboBox combo in _slotCombos)
			{
				if (combo.Items.Count > 0)
				{
					combo.SelectedIndex = 0;
				}
			}
			foreach (RadioButton radio in _slotLeaders)
			{
				radio.Checked = false;
			}
			lblStatus.Text = "";
		});
	}
}
