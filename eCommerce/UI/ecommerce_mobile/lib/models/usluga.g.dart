// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'usluga.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Usluga _$UslugaFromJson(Map<String, dynamic> json) => Usluga(
  id: (json['id'] as num?)?.toInt(),
  naziv: json['naziv'] as String?,
  opis: json['opis'] as String?,
  cijena: (json['cijena'] as num?)?.toDouble(),
  trajanjeMinuta: (json['trajanjeMinuta'] as num?)?.toInt(),
  uslugaKategorijaId: (json['uslugaKategorijaId'] as num?)?.toInt(),
  uslugaKategorijaNaziv: json['uslugaKategorijaNaziv'] as String?,
  tagovi: json['tagovi'] as String?,
  isActive: json['isActive'] as bool?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  updatedAt: json['updatedAt'] == null
      ? null
      : DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$UslugaToJson(Usluga instance) => <String, dynamic>{
  'id': instance.id,
  'naziv': instance.naziv,
  'opis': instance.opis,
  'cijena': instance.cijena,
  'trajanjeMinuta': instance.trajanjeMinuta,
  'uslugaKategorijaId': instance.uslugaKategorijaId,
  'uslugaKategorijaNaziv': instance.uslugaKategorijaNaziv,
  'tagovi': instance.tagovi,
  'isActive': instance.isActive,
  'createdAt': instance.createdAt?.toIso8601String(),
  'updatedAt': instance.updatedAt?.toIso8601String(),
};
