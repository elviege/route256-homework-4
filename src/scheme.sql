create database route256;

drop table if exists warehouses;
create table warehouses (
    id bigserial constraint pk_warehouses_id primary key ,
    name text
);
insert into warehouses(name)
select md5(random()::text)
from generate_series(1,20) g;

drop table if exists clients;
create table clients (
    id bigserial constraint pk_clients_id primary key,
    name text
);
insert into clients(name)
select md5(random()::text)
from generate_series(1,100) g;

drop table if exists statuses;
create table statuses (
    id serial constraint pk_statuses_id primary key ,
    name text not null 
);
insert into statuses (id, name)
values (1, 'New'), (2, 'InProgress'), (3, 'Pending');

drop table if exists orders;
create table orders (
    id uuid default gen_random_uuid(),
    client_id bigint not null references clients(id),
    creation_dt timestamp not null default current_timestamp,
    issue_dt timestamp null,
    status_id int not null references statuses(id),
    items_data json not null,
    warehouse_id bigint not null references warehouses(id),
    constraint pk_orders_key primary key (id, warehouse_id)
) partition by list (warehouse_id);

drop index if exists ix_orders_status_id;
create index ix_orders_status_id on orders(status_id) include(creation_dt);

create table orders_w1 partition of orders for values in (1);
create table orders_w2 partition of orders for values in (2);
create table orders_w3 partition of orders for values in (3);
create table orders_w4 partition of orders for values in (4);
create table orders_w5 partition of orders for values in (5);
create table orders_w6 partition of orders for values in (6);
create table orders_w7 partition of orders for values in (7);
create table orders_w8 partition of orders for values in (8);
create table orders_w9 partition of orders for values in (9);
create table orders_w10 partition of orders for values in (10);
create table orders_w11 partition of orders for values in (11);
create table orders_w12 partition of orders for values in (12);
create table orders_w13 partition of orders for values in (13);
create table orders_w14 partition of orders for values in (14);
create table orders_w15 partition of orders for values in (15);
create table orders_w16 partition of orders for values in (16);
create table orders_w17 partition of orders for values in (17);
create table orders_w18 partition of orders for values in (18);
create table orders_w19 partition of orders for values in (19);
create table orders_w20 partition of orders for values in (20);

insert into orders(client_id, status_id, items_data, warehouse_id) 
select 
    c.id,
    1,
    '{"items": [{"item_id":1, "count":1}, {"item_id":2, "count":2}]}'::json,
    (select warehouses.id from warehouses order by random()*g limit 1)
from clients c
    cross join generate_series(1,50) g
where c.id % 10 = 0 or c.id % 10 = 9;
insert into orders(client_id, status_id, items_data, warehouse_id)
select
    c.id,
    2,
    '{"items": [{"item_id":3, "count":3}, {"item_id":4, "count":4}]}'::json,
    (select warehouses.id from warehouses order by random()*g limit 1)
from clients c
         cross join generate_series(1,20) g
where c.id % 10 = 1 or c.id % 10 = 8;
insert into orders(client_id, status_id, items_data, warehouse_id)
select
    c.id,
    3,
    '{"items": [{"item_id":5, "count":5}, {"item_id":6, "count":6}]}'::json,
    (select warehouses.id from warehouses order by random()*g limit 1)
from clients c
         cross join generate_series(1,10) g
where c.id % 10 = 2 or c.id % 10 = 7;
insert into orders(client_id, status_id, items_data, warehouse_id)
select
    c.id,
    1,
    '{"items": [{"item_id":7, "count":8}, {"item_id":9, "count":9}]}'::json,
    (select warehouses.id from warehouses order by random()*g limit 1)
from clients c
         cross join generate_series(1,3) g
where c.id % 10 in (3,4,5,6);