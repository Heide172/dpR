using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeaderPack
{


    /// <summary>
    /// Класс для отправки текстового сообщения и 
    /// информации о пересылаемых байтах следующих последними в потоке сетевых данных.
    /// </summary>
    [Serializable]
    public class HeaderDsc
    {
        public ServiceMessage Message;      //Тип сообщения
        public Guid guid;                   //Ip пользователя
        public int DataSize; // размер передаваемых данных
        public string ext; // полное имя файла 
        public string Hex; // хеш сумма файла
        public int comIndex; // индекс комманды 
    }


    public enum ServiceMessage
    {
        Authorization, // авторизация
        data, // данные
        file, // файлы 
        Server, // авторизация серверного приложения 
        clList, // список клиентов 
        cmd, // консольная команда 
        error, // ошибка 
        clCommand, // команда клиенту 
        servCommand // команда серверу 
    }
   

    public static class TCPPack
    {
        public static int HeaderSize = 4;
        public static int BufferSize = 8192;

    }

}
