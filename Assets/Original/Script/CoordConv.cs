using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

/// <summary>
/// ���W�̕ϊ��p�N���X�B�Ȃ�ׂ��V���v���ɕK�v�ȕϊ������B
/// ���n�n��WGS84���g�p�B
/// �v�Z���@�́u���E���n�n�ƍ��W�ϊ��@��c���v�@���{���ʋ���v���Q�l�ɂ����B
/// https://qiita.com/oho-sugu/items/d1a17c741bb8c443c0fd
/// </summary>
public class CoordConv
{
    private const double a = 6378137.0;
    private const double f = 1.0 / 298.257223563;
    private const double e2 = f * ( 2.0 - f );

    /// <summary>
    /// �ܓx�o�x�iWGS84�j���璼�����W�iECEF�j�ɕϊ����郁�\�b�h�B
    /// </summary>
    /// <param name="b">�ܓx</param>
    /// <param name="l">�o�x</param>
    /// <param name="h">�ȉ~�̍�</param>
    /// <returns>�������W��XYZ�B+Z���k�ɁA+X���q�ߐ��A+Y�����o����</returns>
    public static (double x, double y, double z) BLH2XYZ( double b, double l, double h )
    {
        b = Math.PI * b / 180.0;
        l = Math.PI * l / 180.0;

        double N = a / Math.Sqrt( 1.0 - e2 * Math.Pow( Math.Sin( b ), 2.0 ) );

        return (
            ( N + h ) * Math.Cos( b ) * Math.Cos( l ),
            ( N + h ) * Math.Cos( b ) * Math.Sin( l ),
            ( N * ( 1.0 - e2 ) + h ) * Math.Sin( b )
        );
    }

    /// <summary>
    /// �������W�iECEF�j����ܓx�o�x�iWGS84�j�ɕϊ����郁�\�b�h�B
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="z">Z</param>
    /// <returns>�ܓx�o�x�Bb�ܓx�Al�o�x�Ah�ȉ~�̍�</returns>
    public static (double b, double l, double h) XYZ2BLH( double x, double y, double z )
    {
        double p = Math.Sqrt( x * x + y * y );
        double r = Math.Sqrt( p * p + z * z );
        double mu = Math.Atan( z / p * ( ( 1.0 - f ) + e2 * a / r ) );

        double B = Math.Atan( ( z * ( 1.0 - f ) + e2 * a * Math.Pow( Math.Sin( mu ), 3 ) ) / ( ( 1.0 - f ) * ( p - e2 * a * Math.Pow( Math.Cos( mu ), 3 ) ) ) );
        return (
            180.0 * B / Math.PI,
            180.0 * Math.Atan2( y, x ) / Math.PI,
            p * Math.Cos( B ) + z * Math.Sin( B ) - a * Math.Sqrt( 1.0 - e2 * Math.Pow( Math.Sin( B ), 2 ) )
        );
    }
}
