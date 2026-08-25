using System.Collections.Generic;

namespace vibrance.GUI.common
{
    /// <summary>
    /// The reference expectations for GetVendorFromAdapterName, as literal data. No GUI, no
    /// display devices, no driver files. Run by vibrance.GUI.exe --selftest-gpu.
    ///
    /// That function decides which GPU vibranceGUI drives on a machine with both drivers
    /// installed, and it is cheap to get subtly wrong. The word-boundary cases below are here
    /// because a bare substring match on "ATI" classified "Workstation Virtual Display" as an AMD
    /// adapter: a virtual display driver would have turned an honest Ambiguous into a confident
    /// wrong answer.
    /// </summary>
    public static class GraphicsAdapterFixture
    {
        public static List<string> Run()
        {
            List<string> lines = new List<string>();
            lines.Add("vibranceGUI graphics adapter self test");
            lines.Add(string.Empty);

            int passed = 0;
            int total = 0;

            lines.Add("Vendor of the adapter name Windows reports:");
            foreach (VendorCase vendorCase in BuildVendorCases())
            {
                GraphicsAdapter actual = GraphicsAdapterHelper.GetVendorFromAdapterName(vendorCase.AdapterName);
                bool isPass = actual == vendorCase.ExpectedVendor;
                total++;
                if (isPass)
                    passed++;

                lines.Add(string.Format("[{0}] {1} got={2} expected={3}",
                    isPass ? "PASS" : "FAIL",
                    Quote(vendorCase.AdapterName).PadRight(36),
                    actual.ToString().PadRight(10),
                    vendorCase.ExpectedVendor));
            }

            lines.Add(string.Empty);
            lines.Add(string.Format("PASSED {0}/{1}", passed, total));
            lines.Add(string.Empty);
            lines.Add("This reads neither the display devices nor the driver files, so it gives the same");
            lines.Add("answer on a build agent as on the machine that reported the bug.");
            return lines;
        }

        private static List<VendorCase> BuildVendorCases()
        {
            List<VendorCase> cases = new List<VendorCase>();

            cases.Add(new VendorCase("NVIDIA GeForce RTX 5070 Ti", GraphicsAdapter.Nvidia));
            cases.Add(new VendorCase("NVIDIA GeForce GTX 1080 Ti", GraphicsAdapter.Nvidia));
            cases.Add(new VendorCase("NVIDIA Quadro P2000", GraphicsAdapter.Nvidia));

            cases.Add(new VendorCase("AMD Radeon(TM) Graphics", GraphicsAdapter.Amd));
            cases.Add(new VendorCase("AMD Radeon RX 7900 XTX", GraphicsAdapter.Amd));
            cases.Add(new VendorCase("Radeon RX 580 Series", GraphicsAdapter.Amd));
            cases.Add(new VendorCase("ATI Technologies Inc.", GraphicsAdapter.Amd));

            // Digits must stay valid word boundaries, or these two real adapter names stop
            // matching the moment the boundary check is added.
            cases.Add(new VendorCase("ATI2VGA", GraphicsAdapter.Amd));
            cases.Add(new VendorCase("AMD780G Integrated Graphics", GraphicsAdapter.Amd));

            // The regression the boundary check exists for: "ATI" inside an ordinary English word.
            // Every one of these was classified as Amd by a bare substring match.
            cases.Add(new VendorCase("Workstation Virtual Display", GraphicsAdapter.Unknown));
            cases.Add(new VendorCase("Application Virtual Display", GraphicsAdapter.Unknown));
            cases.Add(new VendorCase("Cinematic Display Driver", GraphicsAdapter.Unknown));
            cases.Add(new VendorCase("Innovation Display Adapter", GraphicsAdapter.Unknown));

            // A glued occurrence must not hide a real one later in the same name.
            cases.Add(new VendorCase("Innovation Radeon Display", GraphicsAdapter.Amd));

            // Intel is neither, and saying so is the whole point of the Unknown case.
            cases.Add(new VendorCase("Intel(R) UHD Graphics 770", GraphicsAdapter.Unknown));
            cases.Add(new VendorCase("Intel(R) Iris(R) Xe Graphics", GraphicsAdapter.Unknown));

            cases.Add(new VendorCase("Microsoft Basic Display Adapter", GraphicsAdapter.Unknown));
            cases.Add(new VendorCase("Parsec Virtual Display Adapter", GraphicsAdapter.Unknown));
            cases.Add(new VendorCase("Citrix Indirect Display Adapter", GraphicsAdapter.Unknown));
            cases.Add(new VendorCase("DisplayLink USB Device", GraphicsAdapter.Unknown));

            cases.Add(new VendorCase(string.Empty, GraphicsAdapter.Unknown));
            cases.Add(new VendorCase(null, GraphicsAdapter.Unknown));

            return cases;
        }

        private static string Quote(string adapterName)
        {
            return adapterName == null ? "<null>" : "\"" + adapterName + "\"";
        }

        private class VendorCase
        {
            public VendorCase(string adapterName, GraphicsAdapter expectedVendor)
            {
                this.AdapterName = adapterName;
                this.ExpectedVendor = expectedVendor;
            }

            public string AdapterName { get; private set; }

            public GraphicsAdapter ExpectedVendor { get; private set; }
        }
    }
}
