namespace gui.app.controller.amd
{
    public class AmdAdapter : IGpuAdapter
    {
        public bool IsAvailable()
        {
            if (ADL.ADL_Main_Control_Create != null)
            {
                if (ADL.ADL_SUCCESS == ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1))
                {
                    if (ADL.ADL_Main_Control_Destroy != null)
                    {
                        ADL.ADL_Main_Control_Destroy();
                    }

                    return true;
                }
            }

            return false;
        }
    }
}