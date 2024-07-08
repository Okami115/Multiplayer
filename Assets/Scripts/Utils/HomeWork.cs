
public class HomeWork
{
    //---------------------- Notas ---------------------------


    //------------------Primer Parcial------------------------

    // Leer el tipo de mensaje  - Check

    // Lista de Players - Check

    // Clientes deben tener ID - Check

    // HandShake C2S S2C - Check

    // Ping - Pong - Check

    // Calculo de latencia - Check

    // Manejo de desconexiones - Check

    // Checksums - Check

    // Mensajes Ordenables - Check

    // Importantes - Pendiente

    //------------------Segundo Parcial------------------------

    // Cliente Autoritativo - Check

    // Server not player - Check

    // Server Indpendiente - Check

    // INetObj / NetObj - Check

    // Libreria Comun - Check

    // Reflection - Check

    // MatchMaker Independiente - Pendiente 

    // Agregar todos los tipos de datos - Pendientes

    // El juego - Pendiente

    // FactoryDelete - Pendiente

    //-------------------------FINAL---------------------------

    /*
     
            for (int i = 0; i < netObjets.Count; i++)
            {
                try
                {
                    UtilsTools.LOG("Obj : " + netObjets[i].getID() + " : Owner " + netObjets[i].getOwner() + " : I = " + i);
                    Type type = netObjets[i].GetType();
                    UtilsTools.LOG("Type : " + type.ToString());

                    foreach (FieldInfo info in type.GetFields(
                        BindingFlags.NonPublic |
                        BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        object obj = info.GetValue(netObjets[i]);
                        NetValue netValueAttribute = info.GetCustomAttribute<NetValue>();

                        if (obj != null && netValueAttribute != null && netObjets[i].getNetObj().id == ClientManager.Instance.idClient)
                        {
                            UtilsTools.LOG("Property : " + info.ToString() + " : " + obj.GetType());
                            try
                            {
                                UtilsTools.LOG($"Attribute id : {netValueAttribute.id} ");

                                UtilsTools.LOG($"Field Name : {info.Name}, Value : {obj}, Type : {obj.GetType().Name}");

                                UtilsTools.LOG($"Obj Name : {obj.ToString()}, Owner : {netObjets[i].getNetObj().owner}, ID : {netObjets[i].getNetObj().id}");

                                UtilsTools.LOG($"Messge to send : " + messagesTypeDictionary[obj.GetType()].ToString());

                                Type netMsg = messagesTypeDictionary[obj.GetType()];
                                UtilsTools.LOG($"Get class : " + netMsg.ToString());

                                object netMsgInstance = Activator.CreateInstance(netMsg);
                                UtilsTools.LOG($"Get new instance : " + netMsgInstance.ToString());

                                MethodInfo setValueMethod = netMsg.GetMethod("SetData");
                                UtilsTools.LOG($"Get method : " + setValueMethod.ToString());

                                setValueMethod.Invoke(netMsgInstance, new object[] { obj});
                                UtilsTools.LOG($"Set Value with method : " + setValueMethod.ToString() + " to : " + obj);

                                MethodInfo serializerMethod = netMsg.GetMethod("SerializeWithValueID");
                                UtilsTools.LOG($"Get method : " + serializerMethod.ToString());

                                object serializedMessage = serializerMethod.Invoke(netMsgInstance, new object[] { netValueAttribute.id, netObjets[i].getID()});
                                UtilsTools.LOG($"send netObj  net attribute id : {netValueAttribute.id} : net obj id : {netObjets[i].getID()}");

                                ClientManager.Instance.SendToServer(serializedMessage as byte[]);
                            }
                            catch (Exception ex)
                            {
                                UtilsTools.LOG("Error en ReadValue: " + ex.Message);
                            }
                        }
                        else
                        {
                            UtilsTools.LOG("Property : " + info.ToString() + " : NULL");
                        }
                    }
                }
                catch (Exception ex)
                {
                    UtilsTools.LOG("¡¡ERROR!! : " + ex.Message);
                }
            }
        }
     */
}
