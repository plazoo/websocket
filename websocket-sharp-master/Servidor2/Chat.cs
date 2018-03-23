using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Transactions;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;

namespace Example2
{
  public class Chat : WebSocketBehavior
  {
    private string     _name;
    private static int _number = 0;
    private string     _prefix;

    public Chat ()
      : this (null)
    {
    }

    public Chat (string prefix)
    {
      _prefix = !prefix.IsNullOrEmpty () ? prefix : "anon#";
    }

    private string getName ()
    {
      var name = Context.QueryString["name"];
            usp_PostMantSessiones(2, Convert.ToInt32(name));
            return !name.IsNullOrEmpty () ? name : _prefix + getNumber ();
    }

    private static int getNumber ()
    {
      return Interlocked.Increment (ref _number);
    }

    protected override void OnClose (CloseEventArgs e)
    {
            usp_PostMantSessiones(3, Convert.ToInt32(_name));
            Sessions.Broadcast (String.Format ("{0} got logged off...", _name));
    }

    protected override void OnMessage (MessageEventArgs e)
    {
      var idMensaje= usp_PostMantChat(2, Convert.ToInt32(_name), e.Data);
      Sessions.Broadcast(String.Format("{0}: {1}", _name, e.Data + "|" + idMensaje)); 
    }

    protected override void OnOpen ()
    {
      _name = getName ();
    }


        #region INS
        public int usp_PostMantSessiones(int bus, int IdUsuario)
        {
            Int32 returnValue = 0;

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["CnnSGO"].ConnectionString))
                    {
                        cnn.Open();
                        using (var cmd = new SqlCommand("usp_PostMantSessiones", cnn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@bus", SqlDbType.Int).Value = bus;
                            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = IdUsuario;

                            //returnValue = cmd.ExecuteNonQuery();
                            returnValue = Convert.ToInt32(cmd.ExecuteNonQuery());
                        }
                      
                    }
                    if (returnValue >= 1 || returnValue == -1)
                        scope.Complete();
                }
                catch (Exception ex)
                {
                    return 0;
                    throw ex;

                }
            }
            return returnValue;
        } //fin usp_PostMantSessiones
        /*Mant Mensaje*/
        public int usp_PostMantChat(int bus, int IdUsuario, string vcMensaje)
        {
            Int32 returnValue = 0;

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["CnnSGO"].ConnectionString))
                    {
                        cnn.Open();
                        using (var cmd = new SqlCommand("usp_PostMantChat", cnn))
                        {
                            /*
                            IF @bus=1 GOTO Lst
                            IF @bus=2 GOTO Ins
                            IF @bus=3 GOTO Edt
                            IF @bus=4 GOTO Del
                             */
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@bus", SqlDbType.Int).Value = bus;
                            cmd.Parameters.Add("@inPadre", SqlDbType.Int).Value = 0;
                            cmd.Parameters.Add("@vcMensaje", SqlDbType.VarChar).Value = vcMensaje;
                            cmd.Parameters.Add("@UsuarioRegistro", SqlDbType.Int).Value = IdUsuario;

                            //returnValue = cmd.ExecuteNonQuery();
                            returnValue = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    if (returnValue >= 1 || returnValue == -1)
                        scope.Complete();
                }
                catch (Exception ex)
                {
                    return 0;
                    throw ex;

                }
            }
            return returnValue;
        } //fin usp_PostMantChat
        #endregion
    }
}
