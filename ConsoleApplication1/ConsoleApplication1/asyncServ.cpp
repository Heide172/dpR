#pragma once
#include <winsock2.h>
#include <condition_variable>
#include <mutex>
#include <thread>
#include <iostream>
#include "asyncServ.h"

asyncServ::asyncServ()
{
	server = new udpServ(&mutex, &con_var, &isNotified);
}

asyncServ::asyncServ(udpServ* srv)
{
	srv->setAsync(&mutex, &con_var, &isNotified);
	server = srv;
}

void asyncServ::AsyncStart()
{
	
	std::thread thr(&udpServ::Start, server);
	thr.detach(); 
}
void asyncServ::Pause()
{
	isNotified = false;
	if (server != NULL)
		server->SetOnPause();
	else
		std::cout << "Ошибка при попытке приостановить сервер, указателю не присвоено значение." << std::endl;
}
void asyncServ::Continue()
{
	if (!server->isActive) std::cout << "Сервер не запущен. " << std::endl;
	std::unique_lock<std::mutex> locker(mutex);
	isNotified = true;
	con_var.notify_all();
}
void asyncServ::Stop()
{
	if (server != NULL)
		server->Stop();
	else 
		std::cout << "Ошибка при попытке остановить сервер, указателю не присвоено значение." << std::endl;
}
asyncServ::~asyncServ()
{
	server->Stop();
	delete server;
}
