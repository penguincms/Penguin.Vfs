using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeMethods
{
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;
    private const uint FILE_READ_EA = 0x0008;
    private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
            IntPtr templateFile);

    public static string GetFinalPathName(string path)
    {
        IntPtr h = CreateFile(path,
            FILE_READ_EA,
            FileShare.ReadWrite | FileShare.Delete,
            IntPtr.Zero,
            FileMode.Open,
            FILE_FLAG_BACKUP_SEMANTICS,
            IntPtr.Zero);
        if (h == INVALID_HANDLE_VALUE)
        {
            throw new Win32Exception();
        }

        try
        {
            StringBuilder sb = new(1024);
            uint res = GetFinalPathNameByHandle(h, sb, 1024, 0);
            if (res == 0)
            {
                throw new Win32Exception();
            }

            return sb.ToString();
        }
        finally
        {
            _ = CloseHandle(h);
        }
    }

    public static bool TryGetFinalPathName(string path, out string finalPath)
    {
        try
        {
            finalPath = GetFinalPathName(path);
            return true;
        }
        catch
        {
            finalPath = null;
            return false;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);
}