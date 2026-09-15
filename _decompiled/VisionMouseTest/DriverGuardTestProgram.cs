using System;
using InputInterceptorNS;

namespace VisionMouseTest;

internal static class DriverGuardTest
{
    public static void Run()
    {
        // InputInterceptor.Initialize() hic cagrilmadan (surucu/DLL yuklenmemis
        // durumu simule ederek) dogrudan CreateContext() cagiriyoruz. Eskiden
        // bu durumda DllWrapper null oldugu icin NullReferenceException firlatirdi.
        try
        {
            IntPtr context = InputInterceptor.CreateContext();
            Console.WriteLine($"CreateContext() sonucu: {context} (beklenen: IntPtr.Zero, exception FIRLAMAMALI)");
            Console.WriteLine(context == IntPtr.Zero ? "SONUC: PASS - DllWrapper hazir degilken guvenli sekilde IntPtr.Zero dondu, cokme yok." : "SONUC: FAIL - beklenmeyen deger.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("SONUC: FAIL - exception firladi: " + ex);
        }
    }
}
