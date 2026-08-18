# 📋 Lista de Tareas — Proyecto Búnker

> Basada en el GDD `GDD_Proyecto_Bunker_Unity.txt`
> Cada tarea principal (`##`) está dividida en subtareas pequeñas para facilitar
> el desarrollo incremental y permitir cambios futuros sin romper el flujo.

---

## FASE 0 — Configuración Inicial del Proyecto

### 0.1 Estructura del Proyecto Unity
- [x] Crear proyecto Unity (versión URP) ✅
- [x] Configurar estructura de carpetas (`Scripts/`, `ScriptableObjects/`, `Prefabs/`, `UI/`, `Art/`, `Audio/`, `Scenes/`, `Data/`) ✅
- [x] Configurar ajustes de proyecto (resolución, target platform PC, input system) ✅ Input System ya incluido
- [x] Inicializar repositorio Git y `.gitignore` para Unity ✅

### 0.2 Dependencias y Paquetes
- [ ] Evaluar e instalar SDK de mapas (Mapbox Unity SDK u OpenStreetMap alternativo)
- [x] Instalar paquetes necesarios (Input System ya incluido, TextMeshPro pendiente de importar desde Unity) ✅
- [x] Crear escena principal (`MainScene`) y escena de menú (`MenuScene`) ✅

---

## FASE 1 — Prototipo del Bucle Básico (Greybox)

### 1.1 ScriptableObjects de Recursos
- [ ] Crear `ResourceDataSO` (ID, Nombre, EsContaminado, RatioConversión, Icono)
- [ ] Definir todos los recursos básicos: Madera, Chatarra, Minerales, Combustible
- [ ] Definir recursos vitales: Agua Contaminada, Agua Purificada, Comida Contaminada, Comida Limpia
- [ ] Definir recursos tecnológicos: Componentes Electrónicos, Energía Eléctrica, Medicinas/Químicos
- [ ] Definir recursos humanos/sociales: Mano de Obra, Moral/Estabilidad, Influencia/Autoridad

### 1.2 Sistema de Inventario del Búnker
- [ ] Crear clase `ResourceManager` (singleton/servicio) para almacenar cantidades de cada recurso
- [ ] Implementar métodos `AddResource()`, `RemoveResource()`, `HasEnough()`, `GetAmount()`
- [ ] Crear sistema de notificación/eventos cuando un recurso cambia (Observer pattern)
- [ ] Implementar límites de almacenamiento por recurso (capacidad máxima)

### 1.3 Reloj de Simulación (SimulationClock)
- [ ] Crear `SimulationClock` con tick configurable (ej. cada X segundos = 1 hora de juego)
- [ ] Implementar controles de velocidad: pausa, x1, x2, x4
- [ ] Crear sistema de eventos por tick (`OnTickElapsed`) para que otros sistemas suscriban
- [ ] Implementar contador de día/hora visible en UI

### 1.4 Consumo de Población
- [ ] Crear clase `PopulationManager` para gestionar la población total del búnker
- [ ] Implementar consumo automático de Agua Purificada y Comida Limpia por tick
- [ ] Calcular ratio de consumo según tamaño de población
- [ ] Implementar consecuencias por falta de recursos (bajas médicas, reducción de moral)
- [ ] Crear evento de muerte/baja por inanición o deshidratación

### 1.5 Sistema de Purificación Interno
- [ ] Crear clase `PurificationSystem` para convertir recursos contaminados en limpios
- [ ] Implementar cola de purificación (Agua Contaminada → Agua Purificada)
- [ ] Implementar cola de descontaminación (Comida Contaminada → Comida Limpia)
- [ ] Añadir requisitos de personal asignado y energía para que funcione
- [ ] Implementar tiempos de conversión configurables por recurso
- [ ] Penalizar el consumo directo de recursos contaminados (moral baja, enfermedades)

### 1.6 UI Básica del Búnker (Greybox)
- [ ] Crear panel de recursos visible en pantalla (barras o contadores numéricos)
- [ ] Crear panel de población (total, estado de salud, moral)
- [ ] Crear botones de control de velocidad del reloj
- [ ] Crear panel de purificación (colas activas, progreso, personal asignado)
- [ ] Implementar tooltips básicos al pasar el ratón por los recursos

---

## FASE 2 — Sistema de Mapa y Nodos

### 2.1 Integración del Mapa Geoespacial
- [ ] Integrar SDK de mapas en Unity (Mapbox o alternativa)
- [ ] Implementar selección de coordenadas reales por el jugador (pantalla de inicio)
- [ ] Renderizar mapa base 2D/3D en la escena principal
- [ ] Implementar cámara con zoom, pan y rotación sobre el mapa
- [ ] Definir radio inicial del búnker en el mapa

### 2.2 Análisis de Capas Geográficas
- [ ] Implementar lectura de capas vectoriales/ráster del terreno
- [ ] Crear clasificador de zonas: Urbana, Forestal, Cuerpo de Agua, Agrícola
- [ ] Mapear etiquetas OSM a categorías internas (`landuse=forest` → Forestal, etc.)
- [ ] Crear sistema de detección de zonas híbridas (intersección de capas)

### 2.3 ScriptableObjects de Nodos
- [ ] Crear `NodeDataSO` (TipoNodo, RecursosBase, TasaProducción, CosteConstrucción)
- [ ] Definir plantillas: Nodo Urbano, Nodo Forestal, Nodo Acuático, Nodo Agrícola
- [ ] Definir configuraciones híbridas (ej. Río + Bosque = Agua + Comida + Madera)

### 2.4 Generación Procedural de Nodos Naturales
- [ ] Crear algoritmo que recorra el área del mapa y genere nodos según el terreno
- [ ] Instanciar nodos con recursos finitos basados en el tipo de terreno
- [ ] Posicionar nodos en coordenadas del mapa real
- [ ] Implementar visualización de nodos en mapa (iconos, colores por tipo)
- [ ] Crear sistema de densidad para no saturar el mapa (distancia mínima entre nodos)

### 2.5 Clase Base de Nodos (MapNode)
- [ ] Crear clase `MapNode` con posición geográfica, collider interactivo, radio
- [ ] Implementar selección de nodo por click (highlight, panel de info)
- [ ] Crear clase `NaturalNode : MapNode` con gestión de recursos finitos
- [ ] Implementar sistema de agotamiento progresivo del nodo
- [ ] Crear indicador visual del porcentaje de recursos restantes
- [ ] Implementar estado "Agotado" con cambio visual

### 2.6 Sistema de Cuadrillas de Exploración
- [ ] Crear clase `Squad` (equipo de superficie) con miembros, estado, posición
- [ ] Implementar sistema de asignación: seleccionar personal y enviar a un nodo
- [ ] Calcular tiempo de viaje según distancia del nodo al búnker
- [ ] Implementar fases: Viaje de ida → Recolección → Viaje de vuelta
- [ ] Crear barra de progreso para cada cuadrilla activa
- [ ] Transferir recursos recolectados al inventario del búnker al regresar
- [ ] Implementar riesgos durante la expedición (evento aleatorio, pérdida de miembros)

### 2.7 UI del Mapa
- [ ] Crear panel lateral de información del nodo seleccionado
- [ ] Crear panel de cuadrillas activas (destino, estado, progreso)
- [ ] Implementar botón "Enviar cuadrilla" desde el panel de nodo
- [ ] Crear minimapa o vista de radar

---

## FASE 3 — Expansión Territorial e Infraestructura

### 3.1 Puestos de Control (Checkpoints)
- [ ] Crear clase `CheckpointNode : NaturalNode`
- [ ] Implementar mecánica de construcción de puesto sobre un nodo natural existente
- [ ] Definir coste de construcción (recursos + tiempo + personal)
- [ ] Implementar persistencia del puesto aunque el nodo natural se agote
- [ ] Crear funciones del puesto: extensión de radio logístico
- [ ] Añadir requisito de dotación mínima de guardias y consumo de energía
- [ ] Implementar sistema de acogida de supervivientes en el puesto

### 3.2 Radio de Influencia y Red Logística
- [ ] Crear `LogisticsGraphManager` con grafo de conexiones búnker ↔ puestos
- [ ] Implementar cálculo de radio operativo del búnker
- [ ] Extender el radio al construir puestos de control (círculos interconectados)
- [ ] Visualizar en mapa la zona de influencia (overlay semitransparente)
- [ ] Impedir enviar cuadrillas fuera del radio operativo
- [ ] Implementar rutas de suministro entre puestos y búnker

### 3.3 Nodos Creados por el Jugador (Infraestructura Artificial)
- [ ] Crear clase `PlayerConstructedNode : MapNode`
- [ ] Implementar menú de construcción: seleccionar tipo de infraestructura
- [ ] Tipos a implementar:
  - [ ] Planteles Forestales (producción lenta de Madera, requiere tiempo de crecimiento)
  - [ ] Granjas Hidropónicas / Cultivos (producción continua de Comida Limpia)
  - [ ] Minas Profundas (extracción lenta pero inagotable de Minerales)
  - [ ] Planta de Purificación de Agua (externa al búnker)
  - [ ] Refinería de Combustible
- [ ] Implementar fase de maduración/desarrollo (tiempo de construcción + crecimiento)
- [ ] Implementar producción periódica por tick una vez maduro
- [ ] Crear requisitos de personal asignado y energía para operar
- [ ] Visualizar estado de construcción/maduración en el mapa

### 3.4 Nodos Agotados (Reconversión)
- [ ] Detectar automáticamente cuando un nodo natural llega a 0% de recursos
- [ ] Ofrecer opciones al jugador: Demoler, Reacondicionar o Construir encima
- [ ] Implementar mecánica de reconversión de terreno (solar libre para nueva instalación)
- [ ] Crear coste de demolición y tiempo de limpieza

### 3.5 Gestión Interna del Búnker — Roles
- [ ] Crear sistema de asignación de roles para la población:
  - [ ] Trabajadores Internos (purificación, cocina, mantenimiento, laboratorio)
  - [ ] Cuadrillas de Superficie (exploradores, recolectores, constructores)
  - [ ] Guarnición (seguridad interna, dotación de puestos de control)
- [ ] Crear UI para reasignar personal entre roles
- [ ] Implementar efectos de tener demasiado pocos trabajadores en un rol
- [ ] Balancear rendimiento según cantidad de personal asignado

### 3.6 UI de Expansión
- [ ] Crear menú de construcción con lista de edificios disponibles y costes
- [ ] Crear panel de gestión de puestos de control (estado, dotación, radio)
- [ ] Visualizar conexiones de red logística en el mapa
- [ ] Crear alertas cuando un puesto de control pierde dotación o energía

---

## FASE 4 — IA de Búnkeres Rivales y Diplomacia

### 4.1 ScriptableObjects de Facciones
- [ ] Crear `FactionProfileSO` (Personalidad IA, NivelAgresividad, RecursosIniciales)
- [ ] Definir 3-5 perfiles de personalidad (Pacifista, Oportunista, Militarista, etc.)
- [ ] Configurar valores iniciales de cada perfil

### 4.2 Spawneo de Búnkeres Rivales
- [ ] Implementar algoritmo de colocación de búnkeres rivales en el mapa
- [ ] Respetar distancia mínima entre búnkeres
- [ ] Asignar perfil de facción aleatorio o ponderado por zona
- [ ] Crear representación visual del búnker rival en el mapa
- [ ] Implementar "niebla de guerra" (el jugador no ve rivales hasta explorar)

### 4.3 IA de Decisión de los Rivales
- [ ] Crear `AIBunkerController` que tome decisiones por tick
- [ ] Implementar lógica de exploración y recolección de recursos del rival
- [ ] Implementar expansión territorial del rival (construir puestos de control)
- [ ] Crear árbol de decisiones: explorar / expandir / comerciar / atacar
- [ ] Ajustar agresividad según perfil de facción y situación de recursos

### 4.4 Nodos Compartidos — Coexistencia Pacífica
- [ ] Detectar cuando dos búnkeres envían cuadrillas al mismo nodo
- [ ] Implementar protocolo de Coexistencia: dividir rendimiento del nodo entre ambos
- [ ] Crear notificación al jugador cuando un rival llega a un nodo suyo
- [ ] Implementar opción de aceptar coexistencia o iniciar conflicto

### 4.5 Sistema de Conflicto
- [ ] Implementar combate entre cuadrillas en un nodo disputado
- [ ] Crear sistema de combate simple (comparación de fuerza, moral, equipamiento)
- [ ] Implementar costes del conflicto: bajas, munición, moral
- [ ] Crear mecánica de asedio a puestos de control rivales
- [ ] Implementar represalias del rival tras un ataque

### 4.6 Sistema de Diplomacia
- [ ] Crear `DiplomacyManager` para gestionar relaciones entre búnkeres
- [ ] Implementar niveles de relación: Hostil, Neutral, Amigable, Aliado
- [ ] Crear sistema de tratados (pacto de no agresión, comercio, alianza)
- [ ] Implementar ruptura de tratados con consecuencias diplomáticas
- [ ] Crear UI de diplomacia (panel de relaciones, opciones de tratado)

### 4.7 Condiciones de Victoria
- [ ] Implementar sistema de puntuación de Influencia
- [ ] Crear condición de Victoria Diplomática/Reconstructora:
  - [ ] Federar todos los búnkeres mediante tratados
  - [ ] Cubrir todo el territorio con red comercial
- [ ] Crear condición de Victoria Hegemónica:
  - [ ] Conquistar/absorber todos los búnkeres rivales
- [ ] Crear pantalla de fin de partida con resumen

---

## FASE 5 — Arte, UI Estilizada y Pulido

### 5.1 Oficina del Supervisor (Interfaz Principal)
- [ ] Diseñar concepto visual de la oficina (estética diegética/táctica militar)
- [ ] Crear fondo de la oficina con elementos interactivos
- [ ] Implementar transición fluida entre vista de oficina y vista de mapa
- [ ] Añadir elementos decorativos reactivos (pantallas que muestran datos en vivo)

### 5.2 UI Estilizada
- [ ] Rediseñar todos los paneles con estética postapocalíptica/militar
- [ ] Crear iconos finales para cada recurso
- [ ] Diseñar tipografía y paleta de colores coherente
- [ ] Implementar animaciones de transición entre paneles
- [ ] Crear sistema de notificaciones in-game con estilo de radiofrecuencia
- [ ] Diseñar cursor y tooltips temáticos

### 5.3 Arte del Mapa
- [ ] Crear sprites/modelos para cada tipo de nodo
- [ ] Diseñar indicadores de estado de nodos (lleno, parcial, agotado, construido)
- [ ] Crear efectos visuales para la zona de influencia
- [ ] Implementar efecto de contaminación/radiación en el mapa
- [ ] Crear modelos/sprites para cuadrillas en movimiento

### 5.4 Audio y Ambientación
- [ ] Crear/obtener música ambiental postapocalíptica
- [ ] Implementar efectos de sonido para acciones (construir, recolectar, purificar)
- [ ] Crear estática de radiofrecuencia para comunicaciones y alertas
- [ ] Implementar sonidos de ambiente del búnker (maquinaria, ventilación)
- [ ] Crear feedback sonoro para eventos críticos (ataque, agotamiento, muerte)

### 5.5 Eventos Aleatorios
- [ ] Crear sistema de eventos aleatorios por tick
- [ ] Implementar tipos de eventos:
  - [ ] Tormentas tóxicas (dañan cuadrillas en superficie)
  - [ ] Llegada de refugiados (oportunidad de reclutar)
  - [ ] Avería de sistemas internos (requiere reparación urgente)
  - [ ] Descubrimiento de recurso raro
  - [ ] Epidemia interna (consume medicinas)
- [ ] Crear panel de evento con opciones de respuesta para el jugador
- [ ] Implementar consecuencias según la decisión tomada

### 5.6 Balance y Pulido Final
- [ ] Ajustar curvas de consumo de recursos por población
- [ ] Balancear tiempos de crecimiento de infraestructura
- [ ] Balancear agresividad y recursos de la IA rival
- [ ] Ajustar dificultad de los eventos aleatorios
- [ ] Testear bucle de juego completo de principio a fin
- [ ] Optimizar rendimiento (draw calls, pooling, LODs si aplica)
- [ ] Revisar y pulir todos los textos y traducciones

---

## EXTRAS / BACKLOG (Ideas Futuras)

- [ ] Sistema de investigación/tech tree para desbloquear mejoras
- [ ] Modo multijugador (búnkeres controlados por otros jugadores)
- [ ] Editor de búnker interior (diseño de salas y distribución)
- [ ] Sistema de clima dinámico que afecte recolección
- [ ] Mercado negro / comercio entre facciones
- [ ] Sistema de misiones / objetivos secundarios
- [ ] Logros y estadísticas del jugador

---

> **Nota:** Esta lista está pensada para ser un documento vivo. Marca las tareas
> completadas con `[x]`, añade nuevas subtareas cuando surjan y reorganiza
> prioridades según evolucione el proyecto.
