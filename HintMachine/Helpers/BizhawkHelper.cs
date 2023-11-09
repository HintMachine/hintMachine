using HintMachine.Models;

namespace HintMachine.Helpers
{
    static class BizhawkHelper
    {
        public static long GetCurrentFrameCount(ProcessRamWatcher ram)
        {
            // long framecountAddr = ram.ResolvePointerPath64(ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x200, 0x10, 0x38 });
            // return ram.ReadUint32(framecountAddr);
            return 0;
        }
    }
}
