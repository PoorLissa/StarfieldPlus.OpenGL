using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


public class ini_file_base
{
	// map to hold the data
	private Dictionary<string, iniMapData> _dic = null;

	private static string iniFileName = null;
	private static string exeFileName = null;

    private int isChanged = -1;

	// -------------------------------------------------------------------------------------------------------------------

	public struct iniMapData
	{
		public string paramName;
		public string sectionName;
		public string paramData;
		public string comment;
	};

	// -------------------------------------------------------------------------------------------------------------------

	public ini_file_base()
    {
        isChanged = -1;
    }

	// -------------------------------------------------------------------------------------------------------------------

	public string read()
    {
        string err = null;

        try
        {
            read_dic();
        }
        catch (Exception ex)
        {
            err = ex.Message;
        }

        isChanged = 0;

        return err;
    }

	// -------------------------------------------------------------------------------------------------------------------

    public string save()
    {
        string err = null;

        if (isChanged > 0)
        {
            try
            {
                save_dic();
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
        }

        return err;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Get map with all the ini-data
    public Dictionary<string, iniMapData> getDic()
	{
		return _dic;
	}

    // -------------------------------------------------------------------------------------------------------------------

    // Get single parameter by its name (Section_Name.Param_Name)
    public string this[string param]
    {
        get => getValue(param);
        set => setValue(param, value);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Value getter for [public string this[string param]]
    private string getValue(string param)
    {
        if (_dic != null && _dic.ContainsKey(param))
        {
            return _dic[param].paramData;
        }

        return null;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Value setter for [public string this[string param]]
    private void setValue(string param, string value)
    {
        int pos = param.IndexOf('.');

        if (pos > 0)
        {
            isChanged++;

            var sectionName = new StringBuilder(param, 0, pos + 1, pos + 1);
            var paramName   = new StringBuilder(param);
            var paramData   = new StringBuilder(value);
            var comment     = new StringBuilder();

            mapData(sectionName, paramName, paramData, comment);
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Read ini file as a string, as-is
    private void read_raw(ref string res)
	{
		string fileName = get_ini_path();

		if (fileName.Length > 0)
		{
            if (System.IO.File.Exists(fileName))
            {
                res = System.IO.File.ReadAllText(fileName);
            }
		}

		return;
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Read ini file into map
	private void read_dic()
	{
        // Get line bounded by '\n' symbols
        int getLine(ref string src, ref StringBuilder line, ref int pos)
		{
			line.Clear();

            int i = pos;

            if (pos < src.Length)
            {
                pos = src.IndexOf(Environment.NewLine, pos + 1);

                if (pos < 0)
                    pos = src.Length;

                for (; i < pos; i++)
                {
                    line.Append(src[i]);
                }

                if (pos == src.Length)
                    pos = 0;
            }

            trimSB(ref line);

            return pos;
		};


		// ------------------------------------------------------------


		string rawFile = null;
        read_raw(ref rawFile);


		if (rawFile != null && rawFile.Length > 0)
		{
            var lineSB        = new StringBuilder();
            var sectionNameSB = new StringBuilder();
            var paramNameSB   = new StringBuilder();
            var paramDataSB   = new StringBuilder();
            var commentarySB  = new StringBuilder();

            int pos = 0;

            while (getLine(ref rawFile, ref lineSB, ref pos) != 0)
            {
                string line = lineSB.ToString();

				// Empty line indicates the end of current section
				if (line.Length == 0)
				{
                    mapData(sectionNameSB, paramNameSB, paramDataSB, commentarySB);

                    sectionNameSB.Clear();
                    commentarySB.Clear();

                    continue;
				}

				// Commentary
				if (line[0] == '#' || line[0] == ';')
				{
                    if (commentarySB.Length > 0)
                        commentarySB.Append('\n');

                    commentarySB.Append(lineSB);
					continue;
				}

				// [ marks the beginning of section
				if (line[0] == '[')
				{
					int Pos = line.IndexOf(']');
                    sectionNameSB.Append(line, 1, Pos - 1);
                    sectionNameSB.Append('.');
					continue;
				}

				// Text before '=' marks parameter name
				// This is also a point where current record might end
				if (line.IndexOf('=') >= 0)
				{
                    // Save current record (effectively clearing out paramNameSB and paramDataSB)
                    if (paramDataSB.Length != 0)
					{
                        mapData(sectionNameSB, paramNameSB, paramDataSB, commentarySB);
                    }

					// Start the new one
					int Pos = line.IndexOf('=') + 1;

                    // In case '=' is the last char in the line, this line's data seems to be empty: Skip it
                    if (Pos < line.Length)
                    {
                        paramNameSB.Append(line, 0, Pos - 1);
                        trimSB(ref paramNameSB);
                        paramNameSB.Insert(0, sectionNameSB);
                        paramDataSB.Append(line, Pos, line.Length - Pos);
                    }

                    continue;
				}

				// Multiline parameter data:
				{
					paramDataSB.Append(' ');
					paramDataSB.Append(line);
				}
			}

			if (paramNameSB.Length > 0)
			{
				mapData(sectionNameSB, paramNameSB, paramDataSB, commentarySB);
			}
		}

        rawFile = null;

		return;
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Get full path to this program's .exe file
	private string get_exe_path()
	{
		return get_file_path('e');
	}

    // -------------------------------------------------------------------------------------------------------------------

    // Get full path to this program's .ini file
    private string get_ini_path()
	{
		return get_file_path('i');
	}

	// -------------------------------------------------------------------------------------------------------------------

	// Get .exe or .ini fileName
	private string get_file_path(char file)
	{
		string res = null;

		if (iniFileName == null && exeFileName == null)
		{
			exeFileName = AppDomain.CurrentDomain.BaseDirectory + "__ini.exe";
			iniFileName = AppDomain.CurrentDomain.BaseDirectory + "__ini.ini";
		}

		switch (file)
		{
			case 'i':
				res = iniFileName;
				break;

			case 'e':
				res = exeFileName;
				break;
		}

		return res;
	}

    // -------------------------------------------------------------------------------------------------------------------

    private void trimSB(ref StringBuilder sb)
    {
        bool doStop = false;

        // Trim the line
        while (sb.Length > 0 && doStop != true)
        {
            int oldLen = sb.Length;

            if (sb[0] == ' ' || sb[0] == '\t' || sb[0] == '\n' || sb[0] == '\r')
                sb.Remove(0, 1);

            int back = sb.Length - 1;

            if (sb[back] == ' ' || sb[back] == '\t' || sb[back] == '\n' || sb[back] == '\r' || sb[back] == ';')
                sb.Remove(back, 1);

            if (sb.Length == oldLen)
                doStop = true;
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Put the data into map
    // Existing data wth the same key will be replaced
    private void mapData(StringBuilder sectionName, StringBuilder paramName, StringBuilder paramData, StringBuilder comment)
    {
        if (sectionName.Length > 0 || paramName.Length > 0 || paramData.Length > 0 || comment.Length > 0)
        {
            if (_dic == null)
            {
                _dic = new Dictionary<string, iniMapData>();
            }

            trimSB(ref paramData);

            string paramNameStr = paramName.ToString();

            if (_dic.ContainsKey(paramNameStr))
            {
                var data = _dic[paramNameStr];
                data.paramData = paramData.ToString();
                _dic[paramNameStr] = data;
            }
            else
            {
                var data = new iniMapData();

                data.sectionName = sectionName.ToString();
                data.paramName = paramNameStr;
                data.paramData = paramData.ToString();
                data.comment = comment.ToString();

                if (data.sectionName.Length == 0)
                {
                    paramName.Append(comment);
                }

                _dic[paramName.ToString()] = data;
            }

            paramName.Clear();
            paramData.Clear();

            isChanged += isChanged >= 0 ? 1 : 0;
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Save ini-data to file
    private void save_dic()
    {
        if (_dic != null)
        {
            var tmpDic = new Dictionary<string, StringBuilder>();

            foreach (var item in _dic)
            {
                // Comment that does not belong to a section
                if (item.Value.comment != null && item.Value.comment.Length > 0 && item.Value.sectionName.Length == 0)
                {
                    StringBuilder commentSB = new StringBuilder();

                    commentSB.Append(item.Value.comment);
                    commentSB.Append(Environment.NewLine);

                    tmpDic[commentSB.ToString()] = commentSB;
                }

                // Store sections in order to aggregate parameters
                if (item.Value.sectionName != null && item.Value.sectionName.Length > 0)
                {
                    string sectionName = item.Value.sectionName.Substring(0, item.Value.sectionName.Length - 1);

                    StringBuilder sectionSB = null;

                    if (tmpDic.ContainsKey(sectionName))
                    {
                        sectionSB = tmpDic[sectionName];
                    }
                    else
                    {
                        sectionSB = new StringBuilder();

                        if (item.Value.comment.Length > 0)
                        {
                            sectionSB.Append(item.Value.comment);
                            sectionSB.Append(Environment.NewLine);
                        }

                        sectionSB.Append('[');
                        sectionSB.Append(sectionName);
                        sectionSB.Append(']');
                        sectionSB.Append(Environment.NewLine);
                    }

                    bool isParamName = false;

                    if (item.Value.paramName != null && item.Value.paramName.Length > 0)
                    {
                        isParamName = true;
                        int pos = item.Value.paramName.IndexOf('.') + 1;
                        sectionSB.Append(item.Value.paramName, pos, item.Value.paramName.Length - pos);
                        sectionSB.Append(" = ");
                    }

                    if (item.Value.paramData != null && item.Value.paramData.Length > 0)
                    {
                        sectionSB.Append(item.Value.paramData);
                        sectionSB.Append(';');
                        sectionSB.Append(Environment.NewLine);
                    }
                    else
                    {
                        // paramName exists, but the data is actually empty: just add a placeholder
                        if (isParamName)
                        {
                            sectionSB.Append(';');
                            sectionSB.Append(Environment.NewLine);
                        }
                    }

                    tmpDic[sectionName] = sectionSB;
                }
            }

            var data = new StringBuilder();

            foreach (var item in tmpDic)
            {
                data.Append(item.Value);
                data.Append(Environment.NewLine);
            }

            string path = get_ini_path();

            // Make visible (if exists)
            if (System.IO.File.Exists(path))
            {
                FileAttributes attributes = File.GetAttributes(path);
                System.IO.File.SetAttributes(path, attributes & ~FileAttributes.Hidden);
            }

            // Overwrite the file
            System.IO.File.CreateText(path).Dispose();

            using (System.IO.StreamWriter sw = System.IO.File.AppendText(path))
            {
                sw.WriteLine(data);
            }

            // Make hidden
            System.IO.File.SetAttributes(path, FileAttributes.Hidden);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
