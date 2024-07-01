using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Basic launch command processor (Multiplay prefers passing IP and port along)
/// </summary>
public class ApplicationData
{
    // Dictionary to store command-line argument actions
    Dictionary<string, Action<string>> m_CommandDictionary = new Dictionary<string, Action<string>>();

    // Constants for command keys
    const string k_IPCmd = "ip";
    const string k_PortCmd = "port";
    const string k_QueryPortCmd = "queryPort";

    // Static methods to retrieve stored values for IP, Port, and QueryPort
    public static string IP()
    {
        return PlayerPrefs.GetString(k_IPCmd);
    }

    public static int Port()
    {
        return PlayerPrefs.GetInt(k_PortCmd);
    }

    public static int QPort()
    {
        return PlayerPrefs.GetInt(k_QueryPortCmd);
    }

    // Constructor to initialize default values and process command-line arguments
    public ApplicationData()
    {
        // Set default IP, Port, and QueryPort values
        SetIP("127.0.0.1");
        SetPort("7777");
        SetQueryPort("7787");

        // Map command-line argument keys to their respective methods
        m_CommandDictionary["-" + k_IPCmd] = SetIP;
        m_CommandDictionary["-" + k_PortCmd] = SetPort;
        m_CommandDictionary["-" + k_QueryPortCmd] = SetQueryPort;

        // Process the command-line arguments
        ProcessCommandLinearguments(Environment.GetCommandLineArgs());
    }

    // Method to process command-line arguments
    void ProcessCommandLinearguments(string[] args)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Launch Args: ");
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = "";
            if (i + 1 < args.Length) // Check if the next argument exists
                nextArg = args[i + 1];

            if (EvaluatedArgs(arg, nextArg))
            {
                sb.Append(arg);
                sb.Append(" : ");
                sb.AppendLine(nextArg);
                i++; // Skip the next argument as it is part of the current command
            }
        }

        Debug.Log(sb);
    }

    // Method to evaluate and execute command-line arguments
    bool EvaluatedArgs(string arg, string nextArg)
    {
        if (!IsCommand(arg))
            return false;
        if (IsCommand(nextArg)) // Check if the next argument is also a command
        {
            return false;
        }

        // Invoke the command with its argument
        m_CommandDictionary[arg].Invoke(nextArg);
        return true;
    }

    // Method to set the IP address
    void SetIP(string ipArgument)
    {
        PlayerPrefs.SetString(k_IPCmd, ipArgument);
    }

    // Method to set the Port number
    void SetPort(string portArgument)
    {
        if (int.TryParse(portArgument, out int parsedPort))
        {
            PlayerPrefs.SetInt(k_PortCmd, parsedPort);
        }
        else
        {
            Debug.LogError($"{portArgument} does not contain a parseable port!");
        }
    }

    // Method to set the Query Port number
    void SetQueryPort(string qPortArgument)
    {
        if (int.TryParse(qPortArgument, out int parsedQPort))
        {
            PlayerPrefs.SetInt(k_QueryPortCmd, parsedQPort);
        }
        else
        {
            Debug.LogError($"{qPortArgument} does not contain a parseable query port!");
        }
    }

    // Method to check if a string is a valid command
    bool IsCommand(string arg)
    {
        return !string.IsNullOrEmpty(arg) && m_CommandDictionary.ContainsKey(arg) && arg.StartsWith("-");
    }
}
