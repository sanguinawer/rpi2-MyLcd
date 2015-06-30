using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking.Connectivity;

public class Folder
{
    public string text = "";
    public Folder parent = null;
    public List<object> items;
    public Folder(string myName, Folder myParent)
    {
        text = myName;
        parent = myParent;
        items = new List<object>();
    }
}
public class Widget
{
    public string text = "";
    public string function = "";
    public Widget(string myname, string myfunction)
    {
        text = myname;
        function = myfunction;
    }
}
public class Servicio
{
    public string text;
    public string function;
    public List<string> tags;
    public int selected = 0;
    public Servicio(string myName, string myFunction, string myTags)

    {
        text = myName;
        function = myFunction;
        selected = 0;
        if (myTags != "")
        {
            string[] tagsarray = myTags.Split('|');
            tags = new List<string>(tagsarray);
        }
    }
}

class menu
{
    lcd mylcd = null;
    Folder curFolder;
    int curTopItem = 0;
    int curSelectedItem = 0;
    int DISPLAY_ROWS = 2;
    int DISPLAY_COLS = 16;
    public bool bFinaliza = false;
    public void InvokeStringMethod(string methodName)
    {
        // Get the Type for the class
        Type thisType = this.GetType();
        TypeInfo thisTypeInfo = thisType.GetTypeInfo();
        MethodInfo method = thisTypeInfo.GetDeclaredMethod(methodName);
        if (method != null)
        {
            object[] parameters = { };
            method.Invoke(this, parameters);
            
        }
        else
        {
            mylcd.lcdClear();
            mylcd.lcdMessage("No existe func:\n" + methodName);
            WaitLeft();
        }
    }
    void WaitLeft()
    {
        while (true)
            if (mylcd.lcdButtonPressed(lcd.lcdBotones.LEFT))
                break;
    }
    public void funcLocalIPAddress()
    {

        var icp = NetworkInformation.GetInternetConnectionProfile();

        if (icp != null && icp.NetworkAdapter != null)
        {
            var hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                        hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                        && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                        == icp.NetworkAdapter.NetworkAdapterId);

            if (hostname != null)
            {
                // the ip address
                mylcd.lcdClear();
                mylcd.lcdMessage("IP:\n" + hostname.CanonicalName);
                WaitLeft();
            }
        }
        mylcd.lcdClear();
        mylcd.lcdMessage("IP:\nUnknow");
        WaitLeft();
        
    }
    void funcFinalizaPrograma()
    {
        mylcd.lcdClear();
        mylcd.lcdMessage("Are you sure?\nPress Sel for Y");
        while (true)
        {
            if (mylcd.lcdButtonPressed(lcd.lcdBotones.LEFT))
                break;
            if (mylcd.lcdButtonPressed(lcd.lcdBotones.SELECT))
            {
                bFinaliza = true;
                break;
            }
        }
      

    }
    void funcLcdRed()
    {
        mylcd.lcdLedColor(lcd.lcdColores.RED);
    }
    void funcLcdGreen()
    {
        mylcd.lcdLedColor(lcd.lcdColores.GREEN);
    }
    void funcLcdBlue()
    {
        mylcd.lcdLedColor(lcd.lcdColores.BLUE);
    }
    void funcLcdYellow()
    {
        mylcd.lcdLedColor(lcd.lcdColores.YELLOW);
    }
    void funcLcdTeal()
    {
        mylcd.lcdLedColor(lcd.lcdColores.TEAL);
    }
    void funcLcdViolet()
    {
        mylcd.lcdLedColor(lcd.lcdColores.VIOLET );
    }
    void funcLcdWhite()
    {
        mylcd.lcdLedColor(lcd.lcdColores.WHITE);
    }
    void funcLcdNone()
    {
        mylcd.lcdLedColor(lcd.lcdColores.NONE);
    }

    public menu(lcd _lcd, string xml=null)
    {
        
        XmlDocument doc = new XmlDocument();
        if (xml == null)
            doc.LoadXml(
                    "<application>" +
                        "<settings lcdColor=\"blue\" lcdBackground = \"on\"/>" +
                        "<folder text=\"Sistema\">" +
                            "<widget text=\"Get Local IP\" function=\"funcLocalIPAddress\"/>" +
                            "<folder text =\"LCD Color\">" +
                                "<widget text=\"Red\" function=\"funcLcdRed\"/>" +
                                "<widget text=\"Green\" function=\"funcLcdGreen\"/>" +
                                "<widget text=\"Blue\" function=\"funcLcdBlue\" />" +
                                "<widget text=\"Yellow\" function=\"funcLcdYellow\"/>" +
                                "<widget text=\"Teal\" function=\"funcLcdTeal\" />" +
                                "<widget text=\"Violet\" function=\"funcLcdViolet\"/>" +
                                "<widget text=\"White\" function=\"funcLcdWhite\"/>" +
                                "<widget text=\"none\" function= \"funcLcdNone\"/>" +
                            "</folder>" +
                            "<widget text=\"Close\" function=\"funcFinalizaPrograma\"/>" +
                        "</folder>" +
                        "<folder text=\"Menu-2\">" +
                            "<widget text=\"Widget prueba1\" function=\"funcion_prueba1\"/>" +
                            "<widget text=\"Widget prueba2\" function=\"funcion_prueba2\"/>" +
                            "<widget text=\"Widget prueba3\" function=\"funcion_prueba3\"/>" +
                            "<widget text=\"Widget prueba4\" function=\"funcion_prueba4\"/>" +
                            "<service text=\"Service1\" function=\"Wifi\" tag=\"start|stop\"/>" +
                            "<service text=\"Service2\" function=\"Wifi\" tag=\"start|stop|restart\"/>" +
                            "<service text=\"Service3\" function=\"Wifi\" tag=\"start|stop|restart|restop\"/>" +
                        "</folder>" +
                    "</application>");
        else
            doc.LoadXml(xml);
        IXmlNode top = doc.FirstChild;
        Folder uiItems = new Folder("root", null);
        curFolder = uiItems;
        curTopItem = 0;
        curSelectedItem = 0;
        mylcd = _lcd;
        ProcessNode(top, uiItems);
    }
    void HandleSettings(IXmlNode node)
    {
        //global lcd
        if (mylcd == null)
            return;
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "red")
            mylcd.lcdLedColor(lcd.lcdColores.RED);
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "green")
            mylcd.lcdLedColor(lcd.lcdColores.GREEN);
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "blue")
            mylcd.lcdLedColor(lcd.lcdColores.BLUE);
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "yellow")
            mylcd.lcdLedColor(lcd.lcdColores.YELLOW);
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "teal")
            mylcd.lcdLedColor(lcd.lcdColores.TEAL);
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "violet")
            mylcd.lcdLedColor(lcd.lcdColores.VIOLET);
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "white")
            mylcd.lcdLedColor(lcd.lcdColores.WHITE);
        if (node.Attributes.GetNamedItem("lcdColor").NodeValue.ToString().ToLower() == "none")
            mylcd.lcdLedColor(lcd.lcdColores.NONE);

        if (node.Attributes.GetNamedItem("lcdBackground").NodeValue.ToString().ToLower() == "on")
            mylcd.lcdBackLight(1);
        if (node.Attributes.GetNamedItem("lcdBackground").NodeValue.ToString().ToLower() == "off")
            mylcd.lcdBackLight(0);
    }
    void ProcessNode(IXmlNode root, Folder currentItem)
    {
        foreach (IXmlNode nodo in root.ChildNodes)
        {
            string nombre = nodo.NodeName;
            if (nodo.NodeType == NodeType.ElementNode)
            {
                if (nombre == "settings")
                {
                    HandleSettings(nodo);
                }
                else if (nombre == "folder")
                {
                    string NombreFolder = nodo.Attributes.GetNamedItem("text").NodeValue.ToString();
                    Folder thisfolder = new Folder(NombreFolder, currentItem);

                    currentItem.items.Add(thisfolder);
                    ProcessNode(nodo, thisfolder);
                }
                else if (nombre == "widget")
                {
                    string NombreWidget = nodo.Attributes.GetNamedItem("text").NodeValue.ToString();
                    string Funcion = nodo.Attributes.GetNamedItem("function").NodeValue.ToString();
                    Widget thiswidget = new Widget(NombreWidget, Funcion);
                    currentItem.items.Add(thiswidget);

                }
                else if (nombre == "service")
                {
                    string NombreService = nodo.Attributes.GetNamedItem("text").NodeValue.ToString();
                    string FuncionService = nodo.Attributes.GetNamedItem("function").NodeValue.ToString();
                    string TagService = nodo.Attributes.GetNamedItem("tag").NodeValue.ToString();
                    Servicio thisservice = new Servicio(NombreService, FuncionService, TagService);
                    currentItem.items.Add(thisservice);
                  
                }
                else if (nombre == "run")
                {
                    //thisCommand = CommandToRun(child.getAttribute('text'), child.firstChild.data)
                    //currentItem.items.append(thisCommand)
                }  
            }
        }
    }
   
   
    public string display()
    {

        if (curTopItem > curFolder.items.Count - DISPLAY_ROWS)
            curTopItem = curFolder.items.Count - DISPLAY_ROWS;
        if (curTopItem < 0)
            curTopItem = 0;
        string str = "";
        string cmd = "";
        for (int row = curTopItem; row < curTopItem + DISPLAY_ROWS; row++)
        {
            if (row > curTopItem)
                str += '\n';
            if (row < curFolder.items.Count)
            {
                if (row == curSelectedItem)
                {
                    if (curFolder.items[row] is Servicio)
                    {
                        Servicio s = (Servicio)curFolder.items[row];
                        string tagname = s.tags[s.selected];
                        cmd = "-" + s.text + "[" + tagname + "]";
                        str += cmd;
                    }
                    else
                    {
                        string texto = "";
                        if (curFolder.items[row] is Folder)
                            texto = ((Folder)curFolder.items[row]).text;
                        if (curFolder.items[row] is Widget)
                            texto = ((Widget)curFolder.items[row]).text;

                        cmd = "-" + texto;
                        if (cmd.Length < 16)
                            for (int x = cmd.Length; x < 16; x++)
                                cmd += " ";
                        str += cmd;
                    }
                }
                else
                {
                    string texto = "";
                    if (curFolder.items[row] is Folder)
                        texto = ((Folder)curFolder.items[row]).text;
                    if (curFolder.items[row] is Widget)
                        texto = ((Widget)curFolder.items[row]).text;
                    if (curFolder.items[row] is Servicio)
                        texto = ((Servicio)curFolder.items[row]).text;

                    cmd = " " + texto;
                    if (cmd.Length < 16)
                        for (int x = cmd.Length; x < 16; x++)
                            cmd += " ";
                    str += cmd;

                }
            }
        }
        if (mylcd!=null)
        {
            mylcd.lcdClear();
            mylcd.lcdMessage(str); 
        }
        return str;
    }
    public void up()
    {
        if (curSelectedItem == 0)
            return;
        else if (curSelectedItem > curTopItem)
            curSelectedItem--;
        else
        {
            curTopItem--;
            curSelectedItem--;
        }
    }
    public void down()
    {
        if (curSelectedItem + 1 == curFolder.items.Count)
            return;
        else if (curSelectedItem < curTopItem + DISPLAY_ROWS - 1)
            curSelectedItem++;
        else
        {
            curTopItem++;
            curSelectedItem++;
        }
    }
    public void left()
    {
        if (curFolder.items[curSelectedItem] is Servicio)
        {
            Servicio s = (Servicio)curFolder.items[curSelectedItem];
            int ntags = s.tags.Count;
            if (s.selected <= 0)
                s.selected = ntags - 1;
            else
                s.selected--;

        }
        else if (curFolder.parent is Folder)
        {
            //int itemno = 0;
            int index = 0;
            for (int x=0;x<curFolder.parent.items.Count;x++)
            {
                if (curFolder.parent.items[x] is Folder)
                {
                    if (curFolder == curFolder.parent.items[x])
                        index = x;
                  
                }
             

            }
            if (index < curFolder.parent.items.Count)
            {
                curFolder = curFolder.parent;
                curTopItem = index;
                curSelectedItem = index;
            }
            else
            {
                curFolder = curFolder.parent;
                curTopItem = 0;
                curSelectedItem = 0;
            }
        }

    }
   
    public void right()
    {
        if (curFolder.items[curSelectedItem] is Folder)
        {
            curFolder = (Folder)curFolder.items[curSelectedItem];
            curTopItem = 0;
            curSelectedItem = 0;

        }
        else if (curFolder.items[curSelectedItem] is Widget)
        {
            Widget w = (Widget)curFolder.items[curSelectedItem];
            string funcionajecutar = w.function;
            InvokeStringMethod(funcionajecutar);

        }
        else if (curFolder.items[curSelectedItem] is Servicio)
        {
            Servicio w = (Servicio)curFolder.items[curSelectedItem];
            int ntag = w.tags.Count;
            if (w.selected >= ntag - 1)
                w.selected = 0;
            else
                w.selected++;
        }
    }
    public void select()
    {
    }


    public void update(string command)
    {
        //mylcd.lcdBackLight(1);
        //mylcd.lcdLedColor(curre)
        switch (command)
        {
            case "u":
                up();
                break;
            case "d":
                down();
                break;
            case "r":
                right();
                break;
            case "l":
                left();
                break;
            case "s":
                select();
                break;
        }
    }   
}
