# IntEqualsDouble
Procedures for detecting Int32 and Double comparisons under .NET Core 3.1.

������
����������Ĵ����ӷ�ף���Ϊ��ʵ����ֻ����Ҫ���ٵõ�����������õ��˶��߳�ȥ��int����Сֵ���Ե����ֵ��
���Եı���ʽ��i == double.Parse(String.Format("{0}.0", i));
Ҳ�������������ܷ�����������ֵ��double��ȡ�
��ֻ֪����C��׼��sqrt()��1, 4, 9����������һ���᷵��1.0, 2.0, 3.0��
�����Ҿ����룬C#��int��double�ıȽ����Ƿ�ῼ�������������Աȡ�

�����
��֮�����Խ�����ǣ������ǿ��Ե�,��������int��Χ�ڵ������������true��
��������ע�⣺�������������������ıȽϣ���������.NET Core3.1��ò��.NET Framework 4.7.1����ǰ���û��ʲô���⣬����û������Ͼ�ת��.NET Core��ȥ�����ˣ��¡�������

�ɸ����ԣ�
������룬ֻҪ��һ��Cal()��������ݾͿ��Բ��Ա�Ķ�����
��ʵ��EasyFile�Ǵӱ�Ŀ�����ٳ����ģ����Լ��ģ���Ҳ����ã����Ժܷ���Ķ�д���֡�

������
�����ر��˿���̨�������Զ�������ȵ�save.tmp���������޸Ĳ�������ɾ������ļ�����������������ƵĲ��ã���������;�Ĳ�����

���ܣ�
���ҵĵ����ϣ������õĲ�����16,1000000����ʱ���15���ӡ�

�������ݣ�
��֮�� Ҳ �� ���һ��ע�ͣ����������޸��������ȥ����Ĳ��ԡ�
��Flag����һ����ĵġ�