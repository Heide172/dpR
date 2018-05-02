
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include "stdafx.h"
#include <iostream>
#include <condition_variable>
#include <mutex>
#include <thread>
#include "udpServ.h"
#include <WinSock2.h>
#pragma comment(lib,"Ws2_32.lib")


udpServ::udpServ()
{
	isAsync = false;
}
udpServ::udpServ(std::mutex* GetMutex, std::condition_variable* GetCon_var, bool* GetIsNotified)
{
	mutex = GetMutex;
	con_var = GetCon_var;
	isNotified = GetIsNotified;
	isAsync = true;
	isPaused = false;
	isNotified = false;
}

void udpServ::setAsync(std::mutex* GetMutex, std::condition_variable* GetCon_var, bool* GetIsNotified)
{
	if (isActive)
	{
		Stop();
		mutex = GetMutex;
		con_var = GetCon_var;
		isNotified = GetIsNotified;
		isAsync = true;
		isPaused = false;
		Start();
	}
	else
	{
		mutex = GetMutex;
		con_var = GetCon_var;
		isNotified = GetIsNotified;
		isAsync = true;
		isPaused = false;
	}

}
void udpServ::Start()
{
	// инициализация сокета
	
	int err = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (err != 0)
	{
		std::cout << "WSA Startup error " << WSAGetLastError() << std::endl;
	}

	sock = socket(AF_INET, SOCK_DGRAM, 0);

	if (sock == INVALID_SOCKET)
	{
		std::cout << "Ошибка при инициализации сокета: " << WSAGetLastError() << std::endl;
		WSACleanup();
	}

	sockaddr_in sin;
	sin.sin_family = AF_INET;
	sin.sin_addr.s_addr = htonl(INADDR_ANY);
	sin.sin_port = htons(port);

	if (bind(sock, (sockaddr*)&sin, sizeof(sin)) == SOCKET_ERROR)
	{
		std::cout << "Ошибка при привязке: " << WSAGetLastError() << std::endl;
		closesocket(sock);
	}
	isActive = true;
	std::cout << "Сервер запущен. " << std::this_thread::get_id() << std::endl;
	if (isAsync)// цикл обработки подлючений для ассинхронного использования
	{
		while (isActive)
		{
			
			if (isPaused)
			{

				std::cout << "Сервер был приостановлен. " << std::endl;
				std::unique_lock<std::mutex> locker(*mutex);
				while (!(*isNotified))
				{
					con_var->wait(locker); 
				}
				std::cout << "Сервер возобновил работу. " << std::endl;
				isPaused = false;
			}
			sockaddr_in client_addr;
			int client_addr_size = sizeof(client_addr);

			// Connect on
			DWORD val = 1000;
			setsockopt(sock, SOL_SOCKET, SO_RCVTIMEO, (const char*)&val, sizeof DWORD);
			bsize = recvfrom(sock, buff, sizeof(buff), 0, (sockaddr *)&client_addr, &client_addr_size);
			if (bsize == SOCKET_ERROR)
			{
				if (WSAGetLastError() != 10060 && WSAGetLastError() != 10004)
				std::cout << "Пакет не принят: " << WSAGetLastError() << std::endl;
			}
			else
			{
				std::cout << buff << std::endl;

				// Определяем IP-адрес клиента 
				HOSTENT *hostent;
				hostent = gethostbyaddr((char *)&client_addr.sin_addr, 4, AF_INET);
				std::cout << "Соединение с " << inet_ntoa(client_addr.sin_addr) << " port: " << ntohs(client_addr.sin_port) << std::endl;
			}
		}
	}
	else// цикл обработки подключений для синхронного использования
	{
		while (isActive)
		{
			std::cout << "Сервер запущен. " << std::this_thread::get_id() << std::endl;
			sockaddr_in client_addr;
			int client_addr_size = sizeof(client_addr);

			// Connect on
			bsize = recvfrom(sock, buff, sizeof(buff), 0, (sockaddr *)&client_addr, &client_addr_size);
			if (bsize == SOCKET_ERROR)
			{
				if (WSAGetLastError() != 10060 && WSAGetLastError() != 10004)
					std::cout << "Пакет не принят: " << WSAGetLastError() << std::endl;
			}
			else
			{
				std::cout << buff << std::endl;

				// Определяем IP-адрес клиента 
				HOSTENT *hostent;
				hostent = gethostbyaddr((char *)&client_addr.sin_addr, 4, AF_INET);
				std::cout << "Соединение с " << inet_ntoa(client_addr.sin_addr) << " port: " << ntohs(client_addr.sin_port) << std::endl;
			}
		}
	}
	

}

void udpServ::SetOnPause()
{
	if (!isActive) std::cout << "Сервер не запущен. " << std::endl;
	else isPaused = true;
}
void udpServ::Stop()
{
	std::cout << "Сервер остановлен, очищение ресурсов. " << std::endl;
	isActive = false;
	isPaused = false;
	shutdown(sock, 2);
	closesocket(sock);
	WSACleanup();
}

udpServ::~udpServ()
{
	isActive = false;
	shutdown(sock, 2);
	closesocket(sock);
	WSACleanup();
}
