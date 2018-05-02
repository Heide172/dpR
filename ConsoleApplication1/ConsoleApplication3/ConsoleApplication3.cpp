
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "stdafx.h"
#include <iostream>
#include <condition_variable>
#include <mutex>
#include <thread>
#include <WinSock2.h>
#pragma comment(lib,"Ws2_32.lib")

using namespace std;
//------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	setlocale(LC_ALL, "Russian");
	char SERVERADDR[5];
	int PORT;
	char buff[10 * 1014];
	int n; // byte set/recv
	int count; // ���������� ��������� �������
	int val; // �������� �������� / �� ������� ���������
	int index; // ������ � �������
	char hello[70] = "������ ����������� � �������...";
	// for convert 
	char str[256]; //������ � ������� ����� ��������������� �����
	int radix = 10; // ������� ���������
	char *p; // ��������� �� ��������� �������������� int to char

	//-----------------------------------------------------------------------------------------------
	//--- ����� ---
	cout << "                    *********************************************" << endl;
	cout << "                    **             UDPKlient v01               **" << endl;
	cout << "                    *********************************************" << endl;
	//------------------------------------------------------------------------------------------------
	// ������������� WinSock
	WORD wVersionRequested = MAKEWORD(2, 2);
	WSADATA wsaData;
	int err = WSAStartup(wVersionRequested, &wsaData);
	if (err != 0)
	{
		cout << "WSAStartup error: " << WSAGetLastError() << endl;
	}
	// �������� � �������� ������
	SOCKET my_sock = socket(AF_INET, SOCK_DGRAM, 0);
	if (my_sock == INVALID_SOCKET)
	{
		cout << "socket error: " << WSAGetLastError() << endl;
		WSACleanup();
	}

	cout << "������� IP �������(�������� 127.0.0.1): " << endl;
	cin >> SERVERADDR;
	cout << "������� ����� �����(�������� 5150)" << endl;
	cin >> PORT;
	//����� ��������� � ��������
	HOSTENT *hostent;
	sockaddr_in dest_addr;

	dest_addr.sin_family = AF_INET;
	//dest_addr.sin_port=htons(5150);
	dest_addr.sin_port = htons(PORT);

	//����������� IP-������ ����
	if (inet_addr(SERVERADDR))
		dest_addr.sin_addr.s_addr = inet_addr(SERVERADDR);
	else
		if (hostent = gethostbyname(SERVERADDR))
			dest_addr.sin_addr.s_addr = ((unsigned long **)
			hostent->h_addr_list)[0][0];
		else
		{
			cout << "����������� ����: " << WSAGetLastError() << endl;
			closesocket(my_sock);
			WSACleanup();
			return -1;
		}
	// ���������� ����������� �������
	n = sendto(my_sock, hello, sizeof(hello), 0, (sockaddr *)&dest_addr, sizeof(dest_addr));
	system("pause");
	return 0;
}