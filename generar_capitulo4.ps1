# Script para generar el Capítulo 4 reestructurado en Word
$word = New-Object -ComObject Word.Application
$word.Visible = $false
$doc = $word.Documents.Add()

# Configurar márgenes normales
$doc.PageSetup.LeftMargin = $word.CentimetersToPoints(2.5)
$doc.PageSetup.RightMargin = $word.CentimetersToPoints(2.5)
$doc.PageSetup.TopMargin = $word.CentimetersToPoints(2.5)
$doc.PageSetup.BottomMargin = $word.CentimetersToPoints(2.5)

function Add-Paragraph($text, $style="Normal") {
    $p = $doc.Content.Paragraphs.Add()
    $p.Range.Text = $text
    # Buscar el estilo por nombre local para evitar problemas de codificación
    $styleObj = $null
    for ($i=1; $i -le $doc.Styles.Count; $i++) {
        try {
            $s = $doc.Styles($i)
            if ($s.NameLocal -eq $style) {
                $styleObj = $s
                break
            }
        } catch { }
    }
    if ($styleObj -eq $null) { $styleObj = $doc.Styles("Normal") }
    $p.Style = $styleObj
    $p.Range.Font.Name = "Times New Roman"
    if ($style -eq "Normal") {
        $p.Range.Font.Size = 12
        $p.Range.ParagraphFormat.Alignment = 3  # Justificado
        $p.Range.ParagraphFormat.LineSpacingRule = 0
        $p.Range.ParagraphFormat.LineSpacing = 18
        $p.Range.ParagraphFormat.FirstLineIndent = $word.CentimetersToPoints(1.25)
    } elseif ($style -like "Título*") {
        $p.Range.Font.Size = 14
        $p.Range.Font.Bold = $true
        $p.Range.ParagraphFormat.Alignment = 1  # Centrado para títulos
        $p.Range.ParagraphFormat.FirstLineIndent = 0
    }
    return $p
}

function Add-BlankLine() {
    Add-Paragraph "" "Normal"
}

# Título del capítulo
Add-Paragraph "CAPÍTULO IV. RESULTADOS Y EXPERIENCIAS" "Título 1"
Add-BlankLine

# Introducción
Add-Paragraph "El presente capítulo tiene como propósito exponer de manera integral la evaluación del proceso de desarrollo del Sistema CRM Ambiental (Eco-Sales CRM), las experiencias vividas durante la estadía en la empresa Sau Ecolsur Sana y los resultados obtenidos alcanzados con base en los objetivos planteados al inicio de este proyecto. A diferencia del capítulo anterior, en el cual se detalló el procedimiento técnico y metodológico seguido, aquí se privilegia la reflexión sobre el trabajo realizado, el aprendizaje obtenido y la evidencia concreta que demuestra que el sistema cumple con lo prometido." "Normal"
Add-BlankLine

# 4.1 Evaluación del proceso
Add-Paragraph "4.1 Evaluación del proceso" "Título 2"
Add-BlankLine

Add-Paragraph "Durante el periodo de estadía se siguió una metodología de trabajo basada en entregas incrementales, organizando el desarrollo en fases que permitieron avanzar de lo general a lo particular. La primera etapa consistió en un diagnóstico directo con el área de ventas, donde se identificaron los puntos de dolor más importantes: la pérdida de prospectos por falta de seguimiento, la duplicidad de información entre las tres unidades de negocio y la dificultad para generar cotizaciones de manera rápida. Esta etapa fue fundamental porque permitió alinear las expectativas del proyecto con las necesidades reales de los usuarios finales." "Normal"
Add-BlankLine

Add-Paragraph "Una vez claros los requerimientos, se procedió al diseño de la arquitectura del sistema y del modelo de datos. Se decidió utilizar ASP.NET MVC 5 con .NET Framework 4.8 porque la infraestructura tecnológica de la empresa ya se encontraba familiarizada con el ecosistema Microsoft, lo que facilitó tanto el desarrollo como el futuro mantenimiento del sistema. La base de datos se diseñó en SQL Server, buscando centralizar toda la información comercial en un único punto de verdad que eliminara los silos de información que existían anteriormente." "Normal"
Add-BlankLine

Add-Paragraph "La ejecución del proyecto se organizó de forma progresiva. Primero se construyeron los módulos de backend, es decir, la lógica que permite registrar, consultar, modificar y eliminar información. Posteriormente se desarrollaron las vistas con el motor Razor, integrando Bootstrap y jQuery para lograr una interfaz moderna y fácil de usar. Finalmente se realizaron las pruebas de funcionalidad, seguridad y usabilidad, corrigiendo los errores detectados antes de entregar el sistema al área de ventas." "Normal"
Add-BlankLine

Add-Paragraph "En términos generales, el proceso cumplió con los tiempos previstos para la mayor parte de las funcionalidades principales. Sin embargo, algunas características adicionales, como el envío automatizado de correos electrónicos y la generación avanzada de documentos PDF, quedaron establecidas como bases funcionales que podrán completarse en siguientes iteraciones. Esto no afectó el cumplimiento del objetivo central del proyecto, que era entregar un CRM funcional, seguro y alineado a los procesos comerciales de la empresa." "Normal"
Add-BlankLine

Add-Paragraph "Como parte de la evaluación, se puede concluir que la decisión de mantener una arquitectura monolítica con vistas renderizadas desde el servidor fue acertada, ya que redujo la complejidad del proyecto y permitió que el equipo de la empresa pudiera entender y dar seguimiento al avance sin necesidad de conocimientos especializados en tecnologías más complejas." "Normal"
Add-BlankLine

# 4.2 Experiencias vividas
Add-Paragraph "4.2 Experiencias vividas durante el desarrollo" "Título 2"
Add-BlankLine

Add-Paragraph "El desarrollo de este proyecto representó una experiencia de aprendizaje significativa, no solo por los conocimientos técnicos adquiridos, sino también por la oportunidad de trabajar en un entorno empresarial real, donde los requerimientos no siempre están escritos de manera perfecta y donde es necesario adaptarse constantemente a las necesidades del usuario." "Normal"
Add-BlankLine

Add-Paragraph "4.2.1 Experiencias positivas" "Título 3"
Add-BlankLine

Add-Paragraph "Una de las experiencias más gratificantes fue comprender de manera práctica el funcionamiento del patrón Modelo-Vista-Controlador (MVC). Al inicio del proyecto se tenía la idea de utilizar tecnologías más modernas como React o Vite, pero durante las primeras reuniones con el equipo de la empresa se decidió mantener el desarrollo sobre ASP.NET MVC 5 porque resultaba más compatible con la infraestructura existente. Esta decisión, aunque parecía conservadora, terminó siendo muy valiosa porque permitió aprender a organizar el código de forma clara y mantenible." "Normal"
Add-BlankLine

Add-Paragraph "Otra experiencia positiva fue el apoyo recibido por parte de los compañeros del área de tecnología. En momentos en los que se presentaron dudas sobre la configuración de SQL Server o la cadena de conexión hacia Visual Studio, siempre hubo disposición para aclararlas. Esto no solo agilizó el trabajo, sino que también generó un ambiente de colaboración que facilitó el aprendizaje. Ver que el sistema comenzaba a funcionar, que los prospectos se registraban correctamente y que las cotizaciones se generaban de forma automática, generó una gran satisfacción personal y profesional." "Normal"
Add-BlankLine

Add-Paragraph "También resultó muy positivo el hecho de que el proyecto tuviera un impacto ambiental indirecto. Al digitalizar procesos que antes se hacían en papel, como la captura de prospectos, la generación de cotizaciones y el almacenamiento de contratos, se contribuyó a reducir el consumo de papel y tinta en el área comercial. Esto alinea el proyecto con la misión de sostenibilidad que tiene la empresa." "Normal"
Add-BlankLine

Add-Paragraph "4.2.2 Experiencias negativas o dificultades presentadas" "Título 3"
Add-BlankLine

Add-Paragraph "A lo largo del desarrollo también se enfrentaron varias dificultades. Una de las primeras fue la configuración de la conexión entre SQL Server y Visual Studio. Inicialmente no se recordaba la cadena de conexión correcta hacia el servidor, lo que retrasó el inicio del trabajo con la base de datos. Gracias a la ayuda de un compañero del equipo se pudo restablecer la contraseña y ubicar los datos necesarios para conectar el proyecto con el servidor, pero esta situación evidenció la importancia de contar con una documentación clara desde el inicio." "Normal"
Add-BlankLine

Add-Paragraph "Otro reto importante fue el manejo de archivos adjuntos dentro de la base de datos. Al principio se tenía la costumbre de utilizar servicios externos de almacenamiento como Supabase para guardar imágenes, pero en este proyecto se optó por almacenar los archivos directamente en SQL Server mediante arreglos de bytes. Esto implicó aprender un nuevo flujo: leer el archivo como una secuencia de ceros y unos en C#, enviarlo de forma segura mediante parámetros a SQL Server y posteriormente recuperarlo escribiéndolo de nuevo en el disco con su extensión original. Fue un proceso que requirió varias pruebas hasta lograr que funcionara correctamente." "Normal"
Add-BlankLine

Add-Paragraph "Asimismo, la curva de aprendizaje de algunas tecnologías fue más pronunciada de lo esperado. Aunque ya se contaba con conocimientos previos de programación, el trabajo con Stored Procedures, la parametrización de consultas para evitar inyección SQL y la integración de librerías frontend como Chart.js y SweetAlert2 demandó tiempo adicional de estudio y práctica." "Normal"
Add-BlankLine

Add-Paragraph "Finalmente, una dificultad recurrente fue la gestión del tiempo entre las actividades de la estadía y el avance del proyecto de titulación. En ocasiones los requerimientos del área comercial cambiaban o se agregaban nuevas ideas durante las reuniones, lo que obligaba a replantear parte del trabajo. Aunque esto representó un desafío, también sirvió para aprender a negociar prioridades y a documentar cada cambio para evitar confusiones." "Normal"
Add-BlankLine

# 4.3 Resultados obtenidos
Add-Paragraph "4.3 Resultados obtenidos" "Título 2"
Add-BlankLine

Add-Paragraph "A continuación se presentan los resultados alcanzados en el proyecto, organizados de acuerdo con los objetivos planteados. Cada resultado incluye una descripción de lo logrado y la evidencia que lo sustenta." "Normal"
Add-BlankLine

# Resultado 1
Add-Paragraph "4.3.1 Resultado del objetivo de diseño de la arquitectura MVC y la base de datos centralizada" "Título 3"
Add-BlankLine

Add-Paragraph "El resultado fue la construcción de una arquitectura de software organizada bajo el patrón Modelo-Vista-Controlador y una base de datos relacional en SQL Server que centraliza toda la información comercial, operativa y normativa de la empresa. Se diseñaron las tablas necesarias para gestionar prospectos, empresas, sucursales, contactos, cotizaciones, contratos, manifiestos, usuarios, servicios, notificaciones y bitácoras de actividad, estableciendo relaciones de llaves primarias y foráneas que garantizan la integridad de la información." "Normal"
Add-BlankLine

Add-Paragraph "Como evidencia de este resultado se encuentra el modelo relacional documentado en el diccionario de datos del sistema, así como el uso de Stored Procedures para las operaciones críticas. La base de datos CRM_Base funciona como el único punto de verdad corporativo, eliminando la duplicidad de datos entre las unidades de negocio y permitiendo que cualquier usuario autorizado acceda a la misma información actualizada en tiempo real. Adicionalmente, se implementó un plan de respaldo que contempla respaldos completos semanales, diferenciales diarios y respaldos del registro de transacciones, lo cual asegura la recuperación de la información ante cualquier eventualidad." "Normal"
Add-BlankLine

# Resultado 2
Add-Paragraph "4.3.2 Resultado del objetivo de desarrollo de los módulos comerciales: prospectos, cotizaciones, contratos y manifiestos" "Título 3"
Add-BlankLine

Add-Paragraph "El resultado fue el desarrollo de los módulos principales que permiten gestionar el ciclo completo de ventas en el sector ambiental. El módulo de prospectos permite registrar empresas generadoras de residuos, capturar múltiples sucursales y contactos por cliente, asignar vendedores, cambiar estatus comerciales y documentar los motivos de rechazo cuando una oportunidad no se concreta. El módulo de cotizaciones permite estructurar servicios de recolección, transporte y disposición final, generar borradores que pueden guardarse para continuar después, y producir documentos de cotización en formato HTML como paso previo a la generación de PDF." "Normal"
Add-BlankLine

Add-Paragraph "El módulo de contratos permite convertir una cotización aprobada en un acuerdo formal entre la empresa y el cliente, mientras que el módulo de manifiestos documenta la trazabilidad de los servicios ambientales prestados. Como evidencia de este resultado se tiene el funcionamiento de los controladores ProspectosController, CotizacionesController y los procedimientos almacenados asociados, los cuales fueron probados de manera continua para garantizar que los registros maestro-detalle se guardaran de forma íntegra, sin pérdida de datos ni violaciones de llaves foráneas." "Normal"
Add-BlankLine

Add-Paragraph "Durante las pruebas se comprobó que el registro de un prospecto con varias sucursales y contactos se almacenaba correctamente en las tablas relacionales, respetando la integridad referencial. También se validó que un usuario pudiera guardar una cotización incompleta como borrador y retomarla posteriormente, funcionalidad que mejora la productividad del equipo de ventas al evitar perder avances por interrupciones durante la jornada laboral." "Normal"
Add-BlankLine

# Resultado 3
Add-Paragraph "4.3.3 Resultado del objetivo de implementación de la interfaz de usuario, dashboards y análisis de datos" "Título 3"
Add-BlankLine

Add-Paragraph "El resultado fue la creación de una interfaz de usuario moderna, responsiva y alineada a los flujos comerciales reales de la empresa. Se utilizaron vistas Razor (.cshtml) combinadas con HTML5, CSS3, Bootstrap 5, jQuery 3.7.1 y jQuery Validation para lograr una experiencia de usuario fluida. El diseño adopta una paleta de colores con tonos verdes y marrones que refuerzan la identidad sostenible de la empresa y que, al mismo tiempo, cumplen con los criterios de contraste para evitar la fatiga visual durante jornadas largas de trabajo." "Normal"
Add-BlankLine

Add-Paragraph "Se implementaron componentes reutilizables, layouts compartidos en la carpeta Views/Shared, modales controlados con JavaScript nativo, tablas dinámicas con DataTables y gráficas interactivas con Chart.js para el dashboard comercial. Como evidencia de este resultado se encuentran las pantallas de inicio de sesión, el menú principal, el listado de prospectos, el formulario de captura con sucursales y contactos dinámicos, y el dashboard con indicadores clave de rendimiento como la tasa de conversión de prospectos, el origen geográfico de la cartera, el pipeline de oportunidades y el cumplimiento de metas de ventas." "Normal"
Add-BlankLine

Add-Paragraph "Las pruebas de usabilidad se realizaron en diferentes resoluciones, incluyendo escritorio, tablet y teléfono móvil, confirmando que el diseño responsive de Bootstrap ajusta correctamente los menús, tablas y formularios. También se comprobó que las validaciones del lado del cliente funcionaban correctamente, evitando que formularios incompletos llegaran al servidor y mostrando mensajes amigables mediante SweetAlert2 en lugar de alertas nativas del navegador." "Normal"
Add-BlankLine

# Resultado 4
Add-Paragraph "4.3.4 Resultado del objetivo de seguridad de la información, despliegue, política de cero papel y trazabilidad ambiental" "Título 3"
Add-BlankLine

Add-Paragraph "El resultado fue la implementación de un conjunto de medidas de seguridad y buenas prácticas que protegen la información del sistema y alinean el proyecto con los objetivos de sostenibilidad de la empresa. En materia de seguridad, se aplicó autenticación basada en FormsAuthentication y sesiones de ASP.NET, control de acceso basado en roles (administrador, vendedor y cliente), parametrización de consultas mediante Stored Procedures para prevenir inyección SQL, sanitización de contenido dinámico en vistas Razor para mitigar ataques XSS, y protección de formularios con AntiForgeryToken para evitar falsificación de peticiones entre sitios." "Normal"
Add-BlankLine

Add-Paragraph "Como evidencia de este resultado, se realizaron pruebas de seguridad en las que cualquier intento de acceder a vistas administrativas sin haber iniciado sesión fue redirigido exitosamente al módulo de inicio de sesión. También se verificó que la capa de acceso a datos, implementada con ADO.NET y SqlParameter, resiste los ataques clásicos de inyección SQL." "Normal"
Add-BlankLine

Add-Paragraph "En cuanto al despliegue, se configuró un entorno de pruebas o staging sobre Internet Information Services (IIS) con Application Pools dedicados, y se preparó la documentación técnica y de usuario para facilitar la adopción del sistema por parte del personal de ventas y operaciones." "Normal"
Add-BlankLine

Add-Paragraph "Respecto a la política de cero papel y la trazabilidad ambiental, el resultado fue la digitalización de contratos, cotizaciones, manifiestos de recolección, firmas de acuerdos, reportes operativos y evidencia fotográfica, almacenados directamente en la base de datos SQL Server. Esta transformación digital reduce el consumo de papel, cartuchos de tinta, espacio físico de archivo y la huella de carbono administrativa. Asimismo, el sistema establece las bases para generar reportes históricos y certificados digitales que documentan la correcta recolección, transporte, tratamiento y disposición final de los residuos, lo cual apoya el cumplimiento normativo ante dependencias regulatorias y facilita auditorías ambientales." "Normal"
Add-BlankLine

# Cierre del capítulo
Add-Paragraph "En resumen, los resultados obtenidos demuestran que el proyecto Eco-Sales CRM cumple con el objetivo general de centralizar y automatizar la gestión comercial del corporativo Ciclo Ambiental. Si bien algunas funcionalidades quedaron planteadas como mejoras futuras, las funcionalidades principales fueron desarrolladas, probadas y documentadas, dejando una base sólida para que la empresa continúe con la digitalización de sus procesos comerciales y operativos." "Normal"
Add-BlankLine

# Guardar
$savePath = "C:\Users\Joel Pool\Downloads\CRMSistema\Capitulo4_reescrito.docx"
if (Test-Path $savePath) { Remove-Item $savePath }
$doc.SaveAs([ref]$savePath)
$doc.Close()
$word.Quit()

[System.Runtime.Interopservices.Marshal]::ReleaseComObject($doc) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null

"Documento guardado en: $savePath"
