using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Editor_ThemeSelect : MonoBehaviour {

	void Start () {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_mapResources = m_gameMap.GetComponent<GameResources>();
        m_dropdown = GetComponent<Dropdown>();

        m_gameMap.mapLoaded += onMapLoaded;

        m_dropdown.onValueChanged.AddListener(delegate {
            onValueChange(m_dropdown);
        });
    }


	void onValueChange(Dropdown obj)
    {
        string packName = obj.captionText.text;

        if (m_mapResources.resourcePack != packName)
        {
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
    }


    void onMapLoaded()
    {
        for (int i = 0; i < m_dropdown.options.Count; ++i)
        {
            if (m_dropdown.options[i].text == m_mapResources.resourcePack)
            {
                m_dropdown.value = i;
                break;
            }
        }
    }


    private GameMap m_gameMap;
    private GameResources m_mapResources;
    private Dropdown m_dropdown;
}
