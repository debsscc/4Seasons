using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necessário para o método Where

public class GerenciadorDeLocalizacoes : MonoBehaviour
{
    // A lista onde você configurará suas 5 localizações no Inspector
    public List<LocalizacaoDoJogo> locaisConfigurados;

    private void Awake()
    {
        // Certifique-se de que este objeto persista entre as cenas
        DontDestroyOnLoad(gameObject);
    }
    
    // ... (restante do código abaixo)
}