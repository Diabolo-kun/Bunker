// ============================================================================
// MapGenerator.cs — Generador procedural del mapa de terrenos y nodos
// GDD Sección 3: Sistema de Mapa Geoespacial — Generación Procedural
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace Bunker.Map
{
    /// <summary>
    /// Genera proceduralmente un grid de terrenos y una lista de posiciones
    /// de nodos naturales usando capas de Perlin Noise.
    /// Componente separado de MapManager para mantener la lógica pura de generación.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        [Header("── Dimensiones ──")]
        [Tooltip("Ancho del mapa en tiles (cada tile = 500m)")]
        [SerializeField] private int anchoTiles = 60;

        [Tooltip("Alto del mapa en tiles")]
        [SerializeField] private int altoTiles = 40;

        [Header("── Búnker ──")]
        [Tooltip("Radio inicial del búnker en tiles")]
        [SerializeField] private int radioInicialBunker = 15;

        [Header("── Generación ──")]
        [Tooltip("Seed para la generación. 0 = aleatorio")]
        [SerializeField] private int seed = 0;

        [Tooltip("Escala del Perlin Noise (menor = terrenos más grandes)")]
        [Range(0.01f, 0.2f)]
        [SerializeField] private float escalaRuido = 0.08f;

        [Header("── Nodos ──")]
        [Tooltip("Distancia mínima entre nodos en tiles")]
        [SerializeField] private int distanciaMinimaEntreNodos = 3;

        [Tooltip("Probabilidad base de generar un nodo en un tile válido (0-1)")]
        [Range(0.01f, 0.5f)]
        [SerializeField] private float densidadNodos = 0.15f;

        [Header("── Umbrales de Terreno ──")]
        [Tooltip("Por debajo de este valor = agua")]
        [Range(0f, 0.5f)]
        [SerializeField] private float umbralAgua = 0.3f;

        [Tooltip("Por encima de este valor = zona urbana/industrial")]
        [Range(0.5f, 1f)]
        [SerializeField] private float umbralUrbano = 0.7f;

        [Tooltip("Umbral de vegetación para bosque")]
        [Range(0.4f, 0.8f)]
        [SerializeField] private float umbralBosque = 0.5f;

        // ── Propiedades ──────────────────────────────────────────────────

        /// <summary>Ancho del mapa en tiles.</summary>
        public int AnchoTiles => anchoTiles;

        /// <summary>Alto del mapa en tiles.</summary>
        public int AltoTiles => altoTiles;

        /// <summary>Posición del búnker en el grid.</summary>
        public Vector2Int PosicionBunker => new Vector2Int(anchoTiles / 2, altoTiles / 2);

        /// <summary>Radio inicial del búnker en tiles.</summary>
        public int RadioInicialBunker => radioInicialBunker;

        // ── Resultado ────────────────────────────────────────────────────

        /// <summary>Resultado de la última generación.</summary>
        public struct ResultadoGeneracion
        {
            public Core.Data.TerrainType[,] gridTerreno;
            public List<NodoProcedural> nodos;
            public Vector2Int posicionBunker;
            public int radioInicial;
        }

        /// <summary>Datos de un nodo generado proceduralmente.</summary>
        public struct NodoProcedural
        {
            public Vector2Int posicion;
            public Core.Data.TerrainType terreno;
            public float multiplicadorRecursos;
        }

        // ── Generación ───────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta la generación procedural completa del mapa.
        /// </summary>
        public ResultadoGeneracion Generar()
        {
            int seedFinal = seed != 0 ? seed : Random.Range(1, 999999);
            Random.InitState(seedFinal);
            Debug.Log($"[MapGenerator] Generando mapa {anchoTiles}x{altoTiles} con seed {seedFinal}");

            var resultado = new ResultadoGeneracion
            {
                gridTerreno = GenerarGridTerreno(seedFinal),
                nodos = new List<NodoProcedural>(),
                posicionBunker = PosicionBunker,
                radioInicial = radioInicialBunker
            };

            resultado.nodos = GenerarNodos(resultado.gridTerreno, seedFinal);

            Debug.Log($"[MapGenerator] Generados {resultado.nodos.Count} nodos naturales.");
            return resultado;
        }

        // ── Grid de terreno ──────────────────────────────────────────────

        private Core.Data.TerrainType[,] GenerarGridTerreno(int seedVal)
        {
            var grid = new Core.Data.TerrainType[anchoTiles, altoTiles];

            // Offsets aleatorios para cada capa de ruido
            float offsetElevacion = seedVal * 0.7f;
            float offsetVegetacion = seedVal * 1.3f;
            float offsetUrbanismo = seedVal * 2.1f;

            for (int x = 0; x < anchoTiles; x++)
            {
                for (int y = 0; y < altoTiles; y++)
                {
                    // Capa 1: Elevación (agua vs tierra)
                    float elevacion = Mathf.PerlinNoise(
                        (x + offsetElevacion) * escalaRuido,
                        (y + offsetElevacion) * escalaRuido);

                    // Capa 2: Vegetación (bosque vs erial)
                    float vegetacion = Mathf.PerlinNoise(
                        (x + offsetVegetacion) * escalaRuido * 1.2f,
                        (y + offsetVegetacion) * escalaRuido * 1.2f);

                    // Capa 3: Urbanismo (ruinas, industria)
                    float urbanismo = Mathf.PerlinNoise(
                        (x + offsetUrbanismo) * escalaRuido * 0.8f,
                        (y + offsetUrbanismo) * escalaRuido * 0.8f);

                    grid[x, y] = ClasificarTerreno(elevacion, vegetacion, urbanismo);
                }
            }

            // El búnker siempre está en terreno sólido
            Vector2Int bunker = PosicionBunker;
            grid[bunker.x, bunker.y] = Core.Data.TerrainType.Erial;

            return grid;
        }

        private Core.Data.TerrainType ClasificarTerreno(float elevacion, float vegetacion, float urbanismo)
        {
            // Agua: baja elevación
            if (elevacion < umbralAgua)
                return Core.Data.TerrainType.Acuatico;

            // Zona urbana/industrial: alto urbanismo
            if (urbanismo > umbralUrbano)
            {
                // Subclasificar: industrial vs urbano residencial
                return urbanismo > 0.85f
                    ? Core.Data.TerrainType.Industrial
                    : Core.Data.TerrainType.Urbano;
            }

            // Bosque: alta vegetación
            if (vegetacion > umbralBosque)
                return Core.Data.TerrainType.Forestal;

            // Terreno intermedio: agrícola si hay algo de vegetación
            if (vegetacion > 0.35f && elevacion > 0.4f)
                return Core.Data.TerrainType.Agricola;

            // Por defecto: erial
            return Core.Data.TerrainType.Erial;
        }

        // ── Generación de nodos ──────────────────────────────────────────

        private List<NodoProcedural> GenerarNodos(Core.Data.TerrainType[,] grid, int seedVal)
        {
            var nodos = new List<NodoProcedural>();
            var posicionesOcupadas = new HashSet<Vector2Int>();
            Vector2Int bunker = PosicionBunker;

            // No generar nodos en la posición del búnker
            posicionesOcupadas.Add(bunker);

            for (int x = 0; x < anchoTiles; x++)
            {
                for (int y = 0; y < altoTiles; y++)
                {
                    var terreno = grid[x, y];

                    // No generar nodos en erial
                    if (terreno == Core.Data.TerrainType.Erial) continue;

                    var pos = new Vector2Int(x, y);

                    // ¿Demasiado cerca de otro nodo?
                    if (EstaCercaDeOtroNodo(pos, posicionesOcupadas)) continue;

                    // Probabilidad de generar nodo
                    float ruido = Mathf.PerlinNoise(
                        (x + seedVal * 3.7f) * 0.15f,
                        (y + seedVal * 3.7f) * 0.15f);

                    if (ruido > (1f - densidadNodos)) 
                    {
                        float multiplicador = Random.Range(0.7f, 1.3f);

                        nodos.Add(new NodoProcedural
                        {
                            posicion = pos,
                            terreno = terreno,
                            multiplicadorRecursos = multiplicador
                        });

                        posicionesOcupadas.Add(pos);
                    }
                }
            }

            return nodos;
        }

        private bool EstaCercaDeOtroNodo(Vector2Int pos, HashSet<Vector2Int> ocupadas)
        {
            foreach (var ocupada in ocupadas)
            {
                if (Vector2Int.Distance(pos, ocupada) < distanciaMinimaEntreNodos)
                    return true;
            }
            return false;
        }
    }
}
