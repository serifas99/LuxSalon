// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'usluga_kategorija.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UslugaKategorija _$UslugaKategorijaFromJson(Map<String, dynamic> json) =>
    UslugaKategorija(
      id: (json['id'] as num?)?.toInt(),
      naziv: json['naziv'] as String?,
      opis: json['opis'] as String?,
      isActive: json['isActive'] as bool?,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$UslugaKategorijaToJson(UslugaKategorija instance) =>
    <String, dynamic>{
      'id': instance.id,
      'naziv': instance.naziv,
      'opis': instance.opis,
      'isActive': instance.isActive,
      'createdAt': instance.createdAt?.toIso8601String(),
    };
