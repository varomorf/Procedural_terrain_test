using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LogWriter
{
    
    public static void Log(string message) {
        try{
            StreamWriter fileWriter = new StreamWriter(Application.persistentDataPath + "/out.log", true);

            fileWriter.Write(message + "\n");

            fileWriter.Close();
        } catch(System.Exception e){
            throw e;
        }
    }

}
