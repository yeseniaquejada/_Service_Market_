using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SERVICE_MARKET.Models
{
    public class multipleModel
    {
        /*CIUDADES*/
        public int ID_CIUDAD { get; set; }
        public string NOMBRE_CIUDAD { get; set; }

        /*USUARIOS*/
        public int ID_USUARIO { get; set; }
        public string NOMBRE_COMPLETO_USU { get; set; }
        public string CELULAR_USU { get; set; }
        public int ID_CIUDAD_FK { get; set; }
        public string CORREO_ELECTRONICO_USU { get; set; }
        public string CONTRASENA_USU { get; set; }
        public string CONFIRMAR_CONTRASENA_U { get; set; }

        /*CATEGORIAS*/
        public int ID_CATEGORIA { get; set; }
        public string NOMBRE_CAT { get; set; }
        public string DESCRIPCION_CAT { get; set; }

        /*ADMINISTRADORES*/
        public int ID_ADMINISTRADOR { get; set; }
        public string NOMBRE_COMPLETO_ADMIN { get; set; }
        public string CORREO_ELECTRONICO_ADMIN { get; set; }
        public string CONTRASENA_ADMIN { get; set; }
        public string CONFIRMAR_CONTRASENA_A { get; set; }

        /*SERVICIOS*/
        public int ID_SERVICIO { get; set; }
        public string NOMBRE_SER { get; set; }
        public int PRECIO_SER { get; set; }
        public string DESCRIPCION_BREVE { get; set; }
        public string TERMINOS_SER { get; set; }
        public string ESTADO_DS { get; set; }
        public string TIPO { get; set; }
        public string FECHA_PUBLICACION { get; set; }
        public int ID_USUARIO_FK { get; set; }
        public int ID_ADMINISTRADOR_FK { get; set; }
        public int ID_CATEGORIA_FK { get; set; }
    }
}