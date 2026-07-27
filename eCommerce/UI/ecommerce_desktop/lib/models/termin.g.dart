// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'termin.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Termin _$TerminFromJson(Map<String, dynamic> json) => Termin(
  id: (json['id'] as num?)?.toInt(),
  klijentId: (json['klijentId'] as num?)?.toInt(),
  klijentImePrezime: json['klijentImePrezime'] as String?,
  frizerId: (json['frizerId'] as num?)?.toInt(),
  frizerImePrezime: json['frizerImePrezime'] as String?,
  uslugaId: (json['uslugaId'] as num?)?.toInt(),
  uslugaNaziv: json['uslugaNaziv'] as String?,
  datumVrijeme: json['datumVrijeme'] == null
      ? null
      : DateTime.parse(json['datumVrijeme'] as String),
  trajanjeMinuta: (json['trajanjeMinuta'] as num?)?.toInt(),
  cijena: (json['cijena'] as num?)?.toDouble(),
  status: json['status'] as String?,
  placanjeStatus: json['placanjeStatus'] as String?,
  placanjeId: (json['placanjeId'] as num?)?.toInt(),
  napomena: json['napomena'] as String?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  updatedAt: json['updatedAt'] == null
      ? null
      : DateTime.parse(json['updatedAt'] as String),
);

Map<String, dynamic> _$TerminToJson(Termin instance) => <String, dynamic>{
  'id': instance.id,
  'klijentId': instance.klijentId,
  'klijentImePrezime': instance.klijentImePrezime,
  'frizerId': instance.frizerId,
  'frizerImePrezime': instance.frizerImePrezime,
  'uslugaId': instance.uslugaId,
  'uslugaNaziv': instance.uslugaNaziv,
  'datumVrijeme': instance.datumVrijeme?.toIso8601String(),
  'trajanjeMinuta': instance.trajanjeMinuta,
  'cijena': instance.cijena,
  'status': instance.status,
  'placanjeStatus': instance.placanjeStatus,
  'placanjeId': instance.placanjeId,
  'napomena': instance.napomena,
  'createdAt': instance.createdAt?.toIso8601String(),
  'updatedAt': instance.updatedAt?.toIso8601String(),
};
