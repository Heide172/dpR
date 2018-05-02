#pragma once
#include <winsock2.h>
#include <condition_variable>
#include <mutex>
#include <thread>

class udpServ
{
public:
	udpServ(); // конструктор для синхронного запуска 
	udpServ(std::mutex* GetMutex,std::condition_variable* GetCon_var, bool* GetIsNotified); // конструктор для ассинхронного запуска
	void Start();
	void Stop();
	void SetOnPause();
	void setAsync(std::mutex* GetMutex, std::condition_variable* GetCon_var, bool* GetIsNotified);
	bool isActive;
	~udpServ();
	
private: 
	unsigned short int port = 3334; // порт 
	std::mutex* mutex; // указатель на мьютекс передаваемый из вызывающего объекта
	std::condition_variable* con_var; // указатель на условную переменную передаваемую из вызывающего объекта
	bool* isNotified; // флаг снятия блокировки для устранения ложных срабатываний 
	bool isAsync; // флаг режима работы сервера синхронный/асинхронный 

	
	bool isPaused; // флаг постановки в режим ожидания
	WSAData wsaData;
	SOCKET sock;
	char buff[1024];
	int bsize;
};

