using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using ManipulateWalls;
using System;

#if !UNITY_EDITOR
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif


public class LogManager : Singleton<LogManager>
{

    public void SendLogMessage(string filename, string logMessage)
    {
        UDPLogManager.Instance.SendLogMessage(logMessage);
        WriteTextOnLocal(filename, logMessage);
    }

    public void WriteTextOnLocal(string filename, string message)
    {
#if !UNITY_EDITOR
        WriteTextOnLocalAsync(filename, message);
#endif
    }

#if !UNITY_EDITOR
    private async void WriteTextOnLocalAsync(string filename, string message)
    {
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        StorageFile textFileForWrite;
        try
        {
            if (await storageFolder.TryGetItemAsync(filename) == null)
            {
                //Create file
                textFileForWrite = await storageFolder.CreateFileAsync(filename);
            }
            else
            {
                textFileForWrite = await storageFolder.GetFileAsync(filename);
            }

            //Write to file
            await FileIO.AppendTextAsync(textFileForWrite, message + "\n");

            /*
            //Get file
            StorageFile textFileForRead = await storageFolder.GetFileAsync(filename);

            //Read file
            string plainText = "";
            plaintext = await FileIO.ReadTextAsync(textFileForRead);

            Debug.Log(plainText);
            */
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
#endif
}
