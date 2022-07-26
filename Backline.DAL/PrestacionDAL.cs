using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;

namespace Backline.DAL
{
    public class PrestacionDAL
    {
        public static List<Backline.Entidades.Prestacion> ObtenerPrestaciones(int empId)
        {
            List<Backline.Entidades.Prestacion> lista = new List<Backline.Entidades.Prestacion>();
            Database db = DatabaseFactory.CreateDatabase("baseDatosFarmacias");
            DbCommand dbCommand = db.GetStoredProcCommand("SP_PRES_PRESTACIONES_GET");

            db.AddInParameter(dbCommand, "EMP_ID", DbType.Int32, empId);

            IDataReader reader = (IDataReader)db.ExecuteReader(dbCommand);

            try
            {
                int ID = reader.GetOrdinal("ID");
                int DESCRIPCION = reader.GetOrdinal("DESCRIPCION");
                int VALOR = reader.GetOrdinal("VALOR");
                int VALORLIBRE = reader.GetOrdinal("VALOR_LIBRE");

                while (reader.Read())
                {
                    Backline.Entidades.Prestacion OBJ = new Backline.Entidades.Prestacion();
                    //BeginFields
                    OBJ.Id = (int)(!reader.IsDBNull(ID) ? reader.GetValue(ID) : 0);
                    OBJ.Descripcion = (String)(!reader.IsDBNull(DESCRIPCION) ? reader.GetValue(DESCRIPCION) : string.Empty);
                    OBJ.Valor = (int)(!reader.IsDBNull(VALOR) ? reader.GetValue(VALOR) :0);
                    OBJ.ValorLibre = (bool)(!reader.IsDBNull(VALORLIBRE) ? reader.GetValue(VALORLIBRE) : false);
                    //EndFields

                    lista.Add(OBJ);
                }
            }
            catch (Exception ex)
            {
                //GlobalesDAO.InsertErrores(ex);
                throw ex;
            }
            finally
            {
                reader.Close();
            }

            return lista;

        }
    }
}
