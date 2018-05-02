#pragma once
#include <winsock2.h>
#include <condition_variable>
#include <mutex>
#include <thread>
#include "udpServ.h"

class asyncServ
{
 public:
	 asyncServ(); // экземпл€р класса сервера создаетс€ внутри класса ассинхронного сервера
	 asyncServ(udpServ* srv); // экземпл€р класса сервера получаетс€ из вне 
	 void AsyncStart(); // запустить сервера в отдельном потоке 
	 void Pause(); // приостановить сервер, 
	 void Continue(); //возобновить работу сервера 
	 void Stop(); // остановить сервер
	 ~asyncServ();
 private:
	 bool isNotified;
	 udpServ* server;
	 std::mutex mutex;
	 std::condition_variable con_var;
};