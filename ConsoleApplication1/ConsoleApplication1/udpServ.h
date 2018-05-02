#pragma once
#include <winsock2.h>
#include <condition_variable>
#include <mutex>
#include <thread>

class udpServ
{
public:
	udpServ(); // ����������� ��� ����������� ������� 
	udpServ(std::mutex* GetMutex,std::condition_variable* GetCon_var, bool* GetIsNotified); // ����������� ��� ������������� �������
	void Start();
	void Stop();
	void SetOnPause();
	void setAsync(std::mutex* GetMutex, std::condition_variable* GetCon_var, bool* GetIsNotified);
	bool isActive;
	~udpServ();
	
private: 
	unsigned short int port = 3334; // ���� 
	std::mutex* mutex; // ��������� �� ������� ������������ �� ����������� �������
	std::condition_variable* con_var; // ��������� �� �������� ���������� ������������ �� ����������� �������
	bool* isNotified; // ���� ������ ���������� ��� ���������� ������ ������������ 
	bool isAsync; // ���� ������ ������ ������� ����������/����������� 

	
	bool isPaused; // ���� ���������� � ����� ��������
	WSAData wsaData;
	SOCKET sock;
	char buff[1024];
	int bsize;
};

