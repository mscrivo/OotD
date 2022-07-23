// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32;

namespace OotD.Utility;

internal static class RegistryHelper
{
    // never instantiated, only contains static methods

    /// <summary>
    /// Renames a subkey of the passed in registry key since
    /// the frame work totally forgot to include such a handy feature.
    /// </summary>
    /// <param name="parentKey">The RegistryKey that contains the subkey
    /// you want to rename (must be writable)</param>
    /// <param name="subKeyName">The name of the subkey that you want to rename</param>
    /// <param name="newSubKeyName">The new name of the RegistryKey</param>
    /// <returns>True if succeeds</returns>
    public static void RenameSubKey(RegistryKey parentKey, string subKeyName, string newSubKeyName)
    {
        CopyKey(parentKey, subKeyName, newSubKeyName);
        parentKey.DeleteSubKeyTree(subKeyName);
    }

    /// <summary>
    /// Copy a registry key.  The parentKey must be writable.
    /// </summary>
    /// <param name="parentKey"></param>
    /// <param name="keyNameToCopy"></param>
    /// <param name="newKeyName"></param>
    /// <returns></returns>
    private static void CopyKey(RegistryKey parentKey, string keyNameToCopy, string newKeyName)
    {
        //Create new key
        var destinationKey = parentKey.CreateSubKey(newKeyName);

        //Open the sourceKey we are copying from
        var sourceKey = parentKey.OpenSubKey(keyNameToCopy);

        if (sourceKey != null)
        {
            RecursivelyCopyKey(sourceKey, destinationKey);
        }
    }

    private static void RecursivelyCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
    {
        //copy all the values
        foreach (var valueName in sourceKey.GetValueNames())
        {
            var objValue = sourceKey.GetValue(valueName);
            var valKind = sourceKey.GetValueKind(valueName);
            if (objValue != null)
            {
                destinationKey.SetValue(valueName, objValue, valKind);
            }
        }

        //For Each subKey
        //Create a new subKey in destinationKey
        //Call myself
        foreach (var sourceSubKeyName in sourceKey.GetSubKeyNames())
        {
            var sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName);
            var destinationSubKey = destinationKey.CreateSubKey(sourceSubKeyName);

            if (sourceSubKey != null)
            {
                RecursivelyCopyKey(sourceSubKey, destinationSubKey);
            }
        }
    }
}