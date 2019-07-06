using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Editor_ThemeSelect : MonoBehaviour {

	void Start () {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_mapResources = m_gameMap.GetComponent<GameResources>();

        Dropdown m_dropdown = GetComponent<Dropdown>();
        m_dropdown.onValueChanged.AddListener(delegate {
            onValueChange(m_dropdown);
        });
    }
	
	void onValueChange(Dropdown obj)
    {
        string packName = obj.captionText.text;

        m_mapResources.loadResources(packName);

        // Save the map into memory and reload from that save to load in the new resource pack
        using (MemoryStream ms = new MemoryStream())
        {
            using (StreamWriter sw = new StreamWriter(ms))
            {
                m_gameMap.exportMap(sw);

                // Closing the writer will close the memory stream, so flush and open the reader while it is alive
                sw.Flush();
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    m_gameMap.importMap(sr);
                }
            }
        }
    }

    private GameMap m_gameMap;
    private GameResources m_mapResources;
}
