using System;
using System.Windows.Forms;
using UI2;

namespace VisionMouseTest;

internal static class FormManagerTest
{
    public static void Run()
    {
        using Form mainForm = new Form();
        FormManager formManager = new FormManager(mainForm);
        using Form panel = new Form { Text = "TestPanel" };
        formManager.RegisterForm("Test", panel);

        formManager.ShowForm("Test");
        Console.WriteLine($"1) Panel gosterildi. Visible={panel.Visible}, IsDisposed={panel.IsDisposed}");

        // Kullanicinin panelin kendi "X" tusuna basmasini simule ediyoruz.
        panel.Close();
        Console.WriteLine($"2) panel.Close() cagrildi (X tusu simulasyonu). Visible={panel.Visible}, IsDisposed={panel.IsDisposed}");

        if (panel.IsDisposed)
        {
            Console.WriteLine("SONUC: FAIL - panel dispose edildi, tekrar acilamaz.");
            return;
        }

        try
        {
            formManager.ShowForm("Test");
            Console.WriteLine($"3) Panel tekrar gosterildi. Visible={panel.Visible}, IsDisposed={panel.IsDisposed}");
            Console.WriteLine("SONUC: PASS - X tusuna basildiktan sonra panel gizlendi (dispose olmadi) ve tekrar acilabildi.");
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine("SONUC: FAIL - tekrar acmaya calisirken exception: " + ex.Message);
        }
    }
}
