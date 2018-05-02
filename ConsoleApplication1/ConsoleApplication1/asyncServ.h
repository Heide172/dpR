#pragma once
#include <winsock2.h>
#include <condition_variable>
#include <mutex>
#include <thread>
#include "udpServ.h"

class asyncServ
{
 public:
	 asyncServ(); // ��������� ������ ������� ��������� ������ ������ ������������� �������
	 asyncServ(udpServ* srv); // ��������� ������ ������� ���������� �� ��� 
	 void AsyncStart(); // ��������� ������� � ��������� ������ 
	 void Pause(); // ������������� ������, 
	 void Continue(); //����������� ������ ������� 
	 void Stop(); // ���������� ������
	 ~asyncServ();
 private:
	 bool isNotified;
	 udpServ* server;
	 std::mutex mutex;
	 std::condition_variable con_var;
};