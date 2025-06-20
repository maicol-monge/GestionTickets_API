USE [ticketsDB]
GO
/****** Object:  Table [dbo].[archivo_adjunto]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[archivo_adjunto](
	[id_archivo] [int] IDENTITY(1,1) NOT NULL,
	[nombre_archivo] [varchar](50) NOT NULL,
	[ruta_archivo] [varchar](255) NOT NULL,
	[id_ticket] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_archivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[asignacion_ticket]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[asignacion_ticket](
	[id_asignacion] [int] IDENTITY(1,1) NOT NULL,
	[id_ticket] [int] NOT NULL,
	[id_tecnico] [int] NOT NULL,
	[fecha_asignacion] [datetime] NOT NULL,
	[estado_ticket] [char](1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_asignacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[categoria]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[categoria](
	[id_categoria] [int] IDENTITY(1,1) NOT NULL,
	[nombre_categoria] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_categoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[comentario]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[comentario](
	[id_comentario] [int] IDENTITY(1,1) NOT NULL,
	[id_ticket] [int] NOT NULL,
	[id_usuario] [int] NOT NULL,
	[fecha_comentario] [datetime] NOT NULL,
	[contenido] [varchar](2000) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_comentario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[empresa]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[empresa](
	[id_empresa] [int] IDENTITY(1,1) NOT NULL,
	[nombre_empresa] [varchar](150) NOT NULL,
	[direccion] [varchar](100) NOT NULL,
	[nombre_contacto_principal] [varchar](65) NOT NULL,
	[correo] [varchar](50) NOT NULL,
	[telefono] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_empresa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tarea_ticket]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tarea_ticket](
	[id_tarea] [int] IDENTITY(1,1) NOT NULL,
	[id_ticket] [int] NOT NULL,
	[id_usuario] [int] NOT NULL,
	[fecha_tarea] [datetime] NOT NULL,
	[contenido] [varchar](1000) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_tarea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tecnico]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tecnico](
	[id_tecnico] [int] IDENTITY(1,1) NOT NULL,
	[id_usuario] [int] NOT NULL,
	[id_categoria] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_tecnico] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ticket]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ticket](
	[id_ticket] [int] IDENTITY(1,1) NOT NULL,
	[titulo] [varchar](200) NOT NULL,
	[tipo_ticket] [varchar](20) NOT NULL,
	[descripcion] [varchar](2000) NOT NULL,
	[prioridad] [varchar](10) NOT NULL,
	[fecha_creacion] [datetime] NOT NULL,
	[fecha_cierre] [datetime] NULL,
	[id_usuario] [int] NOT NULL,
	[id_categoria] [int] NOT NULL,
	[estado] [char](1) NOT NULL,
	[resolucion] [varchar](2000) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_ticket] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usuario]    Script Date: 27/5/2025 14:39:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usuario](
	[id_usuario] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](50) NOT NULL,
	[apellido] [varchar](50) NOT NULL,
	[correo] [varchar](50) NOT NULL,
	[contrasena] [varchar](255) NOT NULL,
	[telefono] [varchar](20) NOT NULL,
	[tipo_usuario] [varchar](10) NOT NULL,
	[rol] [varchar](20) NOT NULL,
	[id_empresa] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[archivo_adjunto] ON 

INSERT [dbo].[archivo_adjunto] ([id_archivo], [nombre_archivo], [ruta_archivo], [id_ticket]) VALUES (14, N'Tendencias_de_Problemas (1).pdf', N'https://wuluxasrfhivhpxcobxy.supabase.co/storage/v1/object/public/files/1748245124672_Tendencias_de_Problemas_(1).pdf', 1)
INSERT [dbo].[archivo_adjunto] ([id_archivo], [nombre_archivo], [ruta_archivo], [id_ticket]) VALUES (17, N'Dificultad.jpg', N'https://wuluxasrfhivhpxcobxy.supabase.co/storage/v1/object/public/files/1748308976947_Dificultad.jpg', 16)
INSERT [dbo].[archivo_adjunto] ([id_archivo], [nombre_archivo], [ruta_archivo], [id_ticket]) VALUES (18, N'Modo_Juego.jpg', N'https://wuluxasrfhivhpxcobxy.supabase.co/storage/v1/object/public/files/1748311443151_Modo_Juego.jpg', 16)
INSERT [dbo].[archivo_adjunto] ([id_archivo], [nombre_archivo], [ruta_archivo], [id_ticket]) VALUES (19, N'Evidencia fallo.docx', N'https://wuluxasrfhivhpxcobxy.supabase.co/storage/v1/object/public/files/1748367399115_Evidencia_fallo.docx', 18)
INSERT [dbo].[archivo_adjunto] ([id_archivo], [nombre_archivo], [ruta_archivo], [id_ticket]) VALUES (20, N'Evidencia fallo 2.docx', N'https://wuluxasrfhivhpxcobxy.supabase.co/storage/v1/object/public/files/1748372136000_Evidencia_fallo_2.docx', 18)
SET IDENTITY_INSERT [dbo].[archivo_adjunto] OFF
GO
SET IDENTITY_INSERT [dbo].[asignacion_ticket] ON 

INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (1, 1, 1, CAST(N'2025-05-25T13:24:41.817' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (2, 1, 1, CAST(N'2025-05-25T13:36:13.880' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (3, 1, 1, CAST(N'2025-05-25T13:41:41.260' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (4, 1, 1, CAST(N'2025-05-25T13:41:52.507' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (5, 1, 1, CAST(N'2025-05-25T13:42:13.793' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (7, 1, 1, CAST(N'2025-05-25T15:41:42.607' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (8, 1, 2, CAST(N'2025-05-25T15:43:24.323' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (9, 1, 2, CAST(N'2025-05-25T15:46:52.040' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (10, 1, 2, CAST(N'2025-05-25T15:52:15.827' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (11, 1, 2, CAST(N'2025-05-25T19:21:53.567' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (12, 1, 2, CAST(N'2025-05-25T19:24:04.533' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (13, 1, 2, CAST(N'2025-05-25T19:26:00.583' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (14, 1, 1, CAST(N'2025-05-25T19:27:27.117' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (15, 1, 1, CAST(N'2025-05-25T19:29:29.033' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (16, 1, 2, CAST(N'2025-05-25T19:29:36.867' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (17, 1, 2, CAST(N'2025-05-25T19:33:33.780' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (18, 1, 2, CAST(N'2025-05-25T19:33:53.313' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (19, 1, 2, CAST(N'2025-05-25T19:36:40.463' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (20, 1, 2, CAST(N'2025-05-25T19:36:56.537' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (21, 1, 2, CAST(N'2025-05-25T19:38:36.217' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (22, 1, 2, CAST(N'2025-05-25T19:38:44.803' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (23, 1, 2, CAST(N'2025-05-25T20:32:28.910' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (24, 1, 2, CAST(N'2025-05-25T20:34:50.407' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (26, 1, 2, CAST(N'2025-05-25T22:57:03.320' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (27, 1, 1, CAST(N'2025-05-25T22:57:12.890' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (28, 1, 1, CAST(N'2025-05-25T23:15:55.233' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (29, 1, 1, CAST(N'2025-05-25T23:18:05.850' AS DateTime), N'E')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (30, 1, 1, CAST(N'2025-05-25T23:18:19.357' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (31, 1, 1, CAST(N'2025-05-25T23:18:55.823' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (32, 1, 1, CAST(N'2025-05-25T23:21:01.617' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (33, 1, 1, CAST(N'2025-05-25T23:25:56.153' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (34, 1, 1, CAST(N'2025-05-25T23:27:15.727' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (35, 1, 1, CAST(N'2025-05-25T23:33:40.913' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (36, 1, 1, CAST(N'2025-05-25T23:34:13.810' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (38, 1, 1, CAST(N'2025-05-26T00:59:00.350' AS DateTime), N'E')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (39, 1, 1, CAST(N'2025-05-26T01:38:52.343' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (40, 1, 1, CAST(N'2025-05-26T01:39:32.553' AS DateTime), N'E')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (41, 1, 1, CAST(N'2025-05-26T01:39:59.090' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (42, 1, 1, CAST(N'2025-05-26T01:42:51.220' AS DateTime), N'E')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (43, 1, 1, CAST(N'2025-05-26T01:43:04.103' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (44, 1, 1, CAST(N'2025-05-26T01:45:55.770' AS DateTime), N'E')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (45, 1, 1, CAST(N'2025-05-26T01:46:03.517' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (46, 1, 1, CAST(N'2025-05-26T01:48:35.173' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (47, 1, 1, CAST(N'2025-05-26T01:54:06.703' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (48, 1, 1, CAST(N'2025-05-26T01:56:26.390' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (49, 1, 1, CAST(N'2025-05-26T02:09:33.057' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (50, 1, 2, CAST(N'2025-05-26T02:09:41.583' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (51, 1, 2, CAST(N'2025-05-26T02:10:39.987' AS DateTime), N'D')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (52, 1, 1, CAST(N'2025-05-26T02:10:45.923' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (53, 1, 1, CAST(N'2025-05-26T02:11:03.447' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (60, 16, 2, CAST(N'2025-05-26T20:01:24.950' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (61, 16, 2, CAST(N'2025-05-26T20:02:22.610' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (62, 16, 2, CAST(N'2025-05-26T20:03:16.283' AS DateTime), N'E')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (63, 16, 2, CAST(N'2025-05-26T20:04:36.407' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (64, 16, 2, CAST(N'2025-05-26T20:05:22.680' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (65, 18, 3, CAST(N'2025-05-27T12:03:09.417' AS DateTime), N'A')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (66, 18, 3, CAST(N'2025-05-27T12:29:44.620' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (67, 18, 3, CAST(N'2025-05-27T12:35:00.217' AS DateTime), N'E')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (68, 18, 3, CAST(N'2025-05-27T12:56:00.863' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (69, 18, 3, CAST(N'2025-05-27T13:03:16.343' AS DateTime), N'R')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (70, 18, 3, CAST(N'2025-05-27T13:13:03.957' AS DateTime), N'P')
INSERT [dbo].[asignacion_ticket] ([id_asignacion], [id_ticket], [id_tecnico], [fecha_asignacion], [estado_ticket]) VALUES (71, 18, 3, CAST(N'2025-05-27T13:15:10.907' AS DateTime), N'R')
SET IDENTITY_INSERT [dbo].[asignacion_ticket] OFF
GO
SET IDENTITY_INSERT [dbo].[categoria] ON 

INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (1, N'Soporte Técnico')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (2, N'Problemas de Red')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (3, N'Hardware')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (4, N'Software')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (5, N'Acceso y Contraseñas')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (6, N'Errores del Sistema')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (7, N'Actualizaciones y Parcheos')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (8, N'Solicitud de Nuevos Equipos')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (9, N'Consultas Generales')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (10, N'Capacitación')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (11, N'Seguridad Informática')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (12, N'Desempeño del Sistema')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (13, N'Backup y Recuperación')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (14, N'Incidentes de Seguridad')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (15, N'Problemas de Correo Electrónico')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (16, N'Mantenimiento Preventivo')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (17, N'Desarrollo de Aplicaciones')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (18, N'Solicitudes Administrativas')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (19, N'Base de Datos')
INSERT [dbo].[categoria] ([id_categoria], [nombre_categoria]) VALUES (20, N'Otros')
SET IDENTITY_INSERT [dbo].[categoria] OFF
GO
SET IDENTITY_INSERT [dbo].[comentario] ON 

INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (1, 1, 1, CAST(N'2025-05-25T18:03:57.227' AS DateTime), N'Estamos por comenzar :)')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (2, 1, 1, CAST(N'2025-05-25T18:53:03.657' AS DateTime), N'ya ahorita desasigno al técnico')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (3, 1, 1, CAST(N'2025-05-25T19:27:47.003' AS DateTime), N'ahoritita si')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (4, 1, 1, CAST(N'2025-05-25T19:29:23.570' AS DateTime), N'ahora si')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (5, 1, 1, CAST(N'2025-05-25T19:39:08.250' AS DateTime), N'ya esta :p')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (6, 1, 1, CAST(N'2025-05-25T23:23:32.963' AS DateTime), N'ya estuvo señor Monge :)')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (7, 1, 1, CAST(N'2025-05-26T01:40:39.687' AS DateTime), N'holaa')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (8, 1, 1, CAST(N'2025-05-26T01:44:27.507' AS DateTime), N'holis')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (9, 1, 1, CAST(N'2025-05-26T01:45:36.627' AS DateTime), N'hola 2')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (10, 16, 5, CAST(N'2025-05-26T19:25:21.310' AS DateTime), N'Buen día, necesito de su apoyo')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (11, 16, 2, CAST(N'2025-05-26T20:03:01.380' AS DateTime), N'Ya revisé el mouse')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (12, 18, 1, CAST(N'2025-05-27T12:02:37.490' AS DateTime), N'Pronto se asignará al personal')
INSERT [dbo].[comentario] ([id_comentario], [id_ticket], [id_usuario], [fecha_comentario], [contenido]) VALUES (13, 18, 5, CAST(N'2025-05-27T12:56:46.040' AS DateTime), N'Ya le he adjuntado mas evidencia del fallo')
SET IDENTITY_INSERT [dbo].[comentario] OFF
GO
SET IDENTITY_INSERT [dbo].[empresa] ON 

INSERT [dbo].[empresa] ([id_empresa], [nombre_empresa], [direccion], [nombre_contacto_principal], [correo], [telefono]) VALUES (1, N'UNICAES', N'Santa Ana', N'Ma. Vilma Escobar', N'vilma.escobar', N'2456-8965')
INSERT [dbo].[empresa] ([id_empresa], [nombre_empresa], [direccion], [nombre_contacto_principal], [correo], [telefono]) VALUES (3, N'ITCA', N'Sivar', N'Lito', N'lito@itca.edu.sv', N'7894-6548')
INSERT [dbo].[empresa] ([id_empresa], [nombre_empresa], [direccion], [nombre_contacto_principal], [correo], [telefono]) VALUES (4, N'UES', N'San Miguel', N'Maria', N'maria@ues.edu.sv', N'6547-8795')
SET IDENTITY_INSERT [dbo].[empresa] OFF
GO
SET IDENTITY_INSERT [dbo].[tarea_ticket] ON 

INSERT [dbo].[tarea_ticket] ([id_tarea], [id_ticket], [id_usuario], [fecha_tarea], [contenido]) VALUES (1, 1, 1, CAST(N'2025-05-25T22:58:04.183' AS DateTime), N'Hicimos un debug')
INSERT [dbo].[tarea_ticket] ([id_tarea], [id_ticket], [id_usuario], [fecha_tarea], [contenido]) VALUES (5, 16, 2, CAST(N'2025-05-26T20:02:45.267' AS DateTime), N'Revisar el mouse')
INSERT [dbo].[tarea_ticket] ([id_tarea], [id_ticket], [id_usuario], [fecha_tarea], [contenido]) VALUES (6, 18, 6, CAST(N'2025-05-27T12:33:46.450' AS DateTime), N'Pruebas preliminares')
SET IDENTITY_INSERT [dbo].[tarea_ticket] OFF
GO
SET IDENTITY_INSERT [dbo].[tecnico] ON 

INSERT [dbo].[tecnico] ([id_tecnico], [id_usuario], [id_categoria]) VALUES (1, 1, 1)
INSERT [dbo].[tecnico] ([id_tecnico], [id_usuario], [id_categoria]) VALUES (2, 2, 1)
INSERT [dbo].[tecnico] ([id_tecnico], [id_usuario], [id_categoria]) VALUES (3, 6, 4)
SET IDENTITY_INSERT [dbo].[tecnico] OFF
GO
SET IDENTITY_INSERT [dbo].[ticket] ON 

INSERT [dbo].[ticket] ([id_ticket], [titulo], [tipo_ticket], [descripcion], [prioridad], [fecha_creacion], [fecha_cierre], [id_usuario], [id_categoria], [estado], [resolucion]) VALUES (1, N'Falla al subir mis archivos desde el sistema principal', N'fallo', N'Cuando trato de ingresar mis archivos al sistema me lanza un error de procesamiento :(', N'importante', CAST(N'2025-05-24T20:53:00.000' AS DateTime), CAST(N'2025-05-26T02:11:33.167' AS DateTime), 1, 6, N'C', N'hoy si ya esta, segurisimo')
INSERT [dbo].[ticket] ([id_ticket], [titulo], [tipo_ticket], [descripcion], [prioridad], [fecha_creacion], [fecha_cierre], [id_usuario], [id_categoria], [estado], [resolucion]) VALUES (16, N'Fallo de Mouse', N'fallo', N'El clic derecho falla constantemente', N'importante', CAST(N'2025-05-26T19:23:00.000' AS DateTime), CAST(N'2025-05-26T20:06:47.320' AS DateTime), 5, 3, N'C', N'El mouse estaba muy dañado, se cambió por uno nuevo')
INSERT [dbo].[ticket] ([id_ticket], [titulo], [tipo_ticket], [descripcion], [prioridad], [fecha_creacion], [fecha_cierre], [id_usuario], [id_categoria], [estado], [resolucion]) VALUES (17, N'Función de Seguimiento', N'nuevo_servicio', N'Función para agregar tareas', N'baja', CAST(N'2025-05-26T19:37:00.000' AS DateTime), NULL, 2, 4, N'A', NULL)
INSERT [dbo].[ticket] ([id_ticket], [titulo], [tipo_ticket], [descripcion], [prioridad], [fecha_creacion], [fecha_cierre], [id_usuario], [id_categoria], [estado], [resolucion]) VALUES (18, N'Error al cargar un archivo', N'fallo', N'Cuando deseo subir un excel, me da error el sistema', N'critico', CAST(N'2025-05-27T11:36:00.000' AS DateTime), CAST(N'2025-05-27T13:34:39.167' AS DateTime), 5, 4, N'C', N'El error era que el nombre de tu archivo, era muy extenso, trata de renombrarlo, mientras el equipo de desarrollo implementa una mejora para permitir nombre de archivos con mas caracteres.')
SET IDENTITY_INSERT [dbo].[ticket] OFF
GO
SET IDENTITY_INSERT [dbo].[usuario] ON 

INSERT [dbo].[usuario] ([id_usuario], [nombre], [apellido], [correo], [contrasena], [telefono], [tipo_usuario], [rol], [id_empresa]) VALUES (1, N'Maicol', N'Monge', N'maicol.monge@catolica.edu.sv', N'VXsatL23+gRf71QLdwvsY/uxdi2WgZOuIxZl6mNQk4NBOQ68', N'6423-2052', N'interno', N'admin', 1)
INSERT [dbo].[usuario] ([id_usuario], [nombre], [apellido], [correo], [contrasena], [telefono], [tipo_usuario], [rol], [id_empresa]) VALUES (2, N'Josue', N'Santamaria', N'monzagonzales23@gmail.com', N'VXsatL23+gRf71QLdwvsY/uxdi2WgZOuIxZl6mNQk4NBOQ68', N'64232052', N'interno', N'empleado', 1)
INSERT [dbo].[usuario] ([id_usuario], [nombre], [apellido], [correo], [contrasena], [telefono], [tipo_usuario], [rol], [id_empresa]) VALUES (5, N'Luis', N'Lopez', N'maicoljosuemongesantamaria@gmail.com', N'hL57tybEM740LSNKC0NJ2yA099N6yTi9O3YnYWBxwANbRvwD', N'78956512', N'externo', N'cliente', 4)
INSERT [dbo].[usuario] ([id_usuario], [nombre], [apellido], [correo], [contrasena], [telefono], [tipo_usuario], [rol], [id_empresa]) VALUES (6, N'Maria', N'Quiñones', N'kiasportage1721@gmail.com', N'7Cw89Mc8d8n6pFsHZS+Qv8a9qtPpHDLkrsPlY9XLLHt2deqH', N'62545465', N'interno', N'empleado', 1)
SET IDENTITY_INSERT [dbo].[usuario] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__usuario__2A586E0BAC339D87]    Script Date: 27/5/2025 14:39:05 ******/
ALTER TABLE [dbo].[usuario] ADD UNIQUE NONCLUSTERED 
(
	[correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[archivo_adjunto]  WITH CHECK ADD FOREIGN KEY([id_ticket])
REFERENCES [dbo].[ticket] ([id_ticket])
GO
ALTER TABLE [dbo].[asignacion_ticket]  WITH CHECK ADD FOREIGN KEY([id_tecnico])
REFERENCES [dbo].[tecnico] ([id_tecnico])
GO
ALTER TABLE [dbo].[asignacion_ticket]  WITH CHECK ADD FOREIGN KEY([id_ticket])
REFERENCES [dbo].[ticket] ([id_ticket])
GO
ALTER TABLE [dbo].[comentario]  WITH CHECK ADD FOREIGN KEY([id_ticket])
REFERENCES [dbo].[ticket] ([id_ticket])
GO
ALTER TABLE [dbo].[comentario]  WITH CHECK ADD FOREIGN KEY([id_usuario])
REFERENCES [dbo].[usuario] ([id_usuario])
GO
ALTER TABLE [dbo].[tarea_ticket]  WITH CHECK ADD FOREIGN KEY([id_ticket])
REFERENCES [dbo].[ticket] ([id_ticket])
GO
ALTER TABLE [dbo].[tarea_ticket]  WITH CHECK ADD FOREIGN KEY([id_usuario])
REFERENCES [dbo].[usuario] ([id_usuario])
GO
ALTER TABLE [dbo].[tecnico]  WITH CHECK ADD FOREIGN KEY([id_categoria])
REFERENCES [dbo].[categoria] ([id_categoria])
GO
ALTER TABLE [dbo].[tecnico]  WITH CHECK ADD FOREIGN KEY([id_usuario])
REFERENCES [dbo].[usuario] ([id_usuario])
GO
ALTER TABLE [dbo].[ticket]  WITH CHECK ADD FOREIGN KEY([id_categoria])
REFERENCES [dbo].[categoria] ([id_categoria])
GO
ALTER TABLE [dbo].[ticket]  WITH CHECK ADD FOREIGN KEY([id_usuario])
REFERENCES [dbo].[usuario] ([id_usuario])
GO
ALTER TABLE [dbo].[usuario]  WITH CHECK ADD FOREIGN KEY([id_empresa])
REFERENCES [dbo].[empresa] ([id_empresa])
GO
ALTER TABLE [dbo].[asignacion_ticket]  WITH CHECK ADD  CONSTRAINT [CHK_estado_ticket] CHECK  (([estado_ticket]='D' OR [estado_ticket]='A' OR [estado_ticket]='R' OR [estado_ticket]='E' OR [estado_ticket]='P'))
GO
ALTER TABLE [dbo].[asignacion_ticket] CHECK CONSTRAINT [CHK_estado_ticket]
GO
ALTER TABLE [dbo].[ticket]  WITH CHECK ADD CHECK  (([estado]='C' OR [estado]='A'))
GO
ALTER TABLE [dbo].[ticket]  WITH CHECK ADD CHECK  (([prioridad]='baja' OR [prioridad]='importante' OR [prioridad]='critico'))
GO
ALTER TABLE [dbo].[ticket]  WITH CHECK ADD CHECK  (([tipo_ticket]='nuevo_servicio' OR [tipo_ticket]='fallo'))
GO
ALTER TABLE [dbo].[usuario]  WITH CHECK ADD  CONSTRAINT [CHK_usuario_rol] CHECK  (([rol]='cliente' OR [rol]='empleado' OR [rol]='admin'))
GO
ALTER TABLE [dbo].[usuario] CHECK CONSTRAINT [CHK_usuario_rol]
GO
ALTER TABLE [dbo].[usuario]  WITH CHECK ADD CHECK  (([tipo_usuario]='externo' OR [tipo_usuario]='interno'))
GO
